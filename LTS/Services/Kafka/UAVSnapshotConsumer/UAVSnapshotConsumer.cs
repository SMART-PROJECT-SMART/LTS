using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Core.Common.Enums;
using LTS.Common;
using LTS.Configuration;
using LTS.Dto;
using LTS.Services.Kafka.UAVSnapshotConsumer.Interfaces;
using LTS.Services.UAVTopicDiscovery.Interfaces;
using Microsoft.Extensions.Options;

namespace LTS.Services.Kafka.UAVSnapshotConsumer
{
    public class UAVSnapshotConsumer : IUAVSnapshotConsumer, IDisposable
    {
        private readonly IConsumer<string, byte[]> _consumer;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IUAVTopicDiscoveryService _topicDiscoveryService;

        public UAVSnapshotConsumer(
            IOptions<KafkaConsumerConfiguration> kafkaConsumerConfiguration,
            IUAVTopicDiscoveryService topicDiscoveryService
        )
        {
            _consumer = BuildConsumer(kafkaConsumerConfiguration.Value);
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
            };
            _topicDiscoveryService = topicDiscoveryService;
        }

        public IEnumerable<UAVTelemetryDataDto> PeekAllUAVSnapshots()
        {
            List<UAVTelemetryDataDto> results = new List<UAVTelemetryDataDto>();
            IEnumerable<int> discoveredUAVIds = _topicDiscoveryService.GetAllCachedUAVIds();

            foreach (int tailId in discoveredUAVIds)
            {
                UAVTelemetryDataDto? snapshot = PeekLatestSnapshotForUAV(tailId);
                if (snapshot != null)
                {
                    results.Add(snapshot);
                }
            }

            return results;
        }

        public void Dispose()
        {
            _consumer.Close();
            _consumer.Dispose();
        }

        private UAVTelemetryDataDto? PeekLatestSnapshotForUAV(int tailId)
        {
            TopicPartition topicPartition = BuildTopicPartition(tailId);

            Offset? latestOffset = GetLatestOffset(topicPartition);
            if (latestOffset == null)
            {
                return null;
            }

            byte[]? messageData = PeekMessageAtOffset(topicPartition, latestOffset.Value);
            if (messageData == null)
            {
                return null;
            }

            IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData =
                DeserializeTelemetryData(messageData);

            return new UAVTelemetryDataDto(tailId, telemetryData.ToDictionary());
        }

        private TopicPartition BuildTopicPartition(int tailId)
        {
            string topicName = $"{LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX}{tailId}";
            return new TopicPartition(
                topicName,
                new Partition(LTSConstants.Kafka.DEFAULT_PARTITION)
            );
        }

        private Offset? GetLatestOffset(TopicPartition topicPartition)
        {
            WatermarkOffsets watermark = _consumer.QueryWatermarkOffsets(
                topicPartition,
                TimeSpan.FromSeconds(LTSConstants.Kafka.CONSUME_TIMEOUT_SECONDS)
            );

            if (watermark.High <= watermark.Low)
            {
                return null;
            }

            if (watermark.High.Value <= 0)
            {
                return null;
            }

            Offset latest = watermark.High - 1;

            if (latest < watermark.Low)
            {
                return null;
            }

            return latest;
        }

        private byte[]? PeekMessageAtOffset(TopicPartition topicPartition, Offset offset)
        {
            _consumer.Assign(new[] { new TopicPartitionOffset(topicPartition, offset) });

            _consumer.Consume(TimeSpan.Zero);

            ConsumeResult<string, byte[]>? result = _consumer.Consume(
                TimeSpan.FromSeconds(LTSConstants.Kafka.CONSUME_TIMEOUT_SECONDS)
            );

            if (result == null || result.IsPartitionEOF)
            {
                return null;
            }

            return result.Message.Value;
        }

        private IEnumerable<KeyValuePair<TelemetryFields, double>> DeserializeTelemetryData(
            byte[] data
        )
        {
            string json = System.Text.Encoding.UTF8.GetString(data);

            Dictionary<TelemetryFields, double>? telemetryDict = JsonSerializer.Deserialize<
                Dictionary<TelemetryFields, double>
            >(json, _jsonSerializerOptions);

            return telemetryDict ?? new Dictionary<TelemetryFields, double>();
        }

        private static IConsumer<string, byte[]> BuildConsumer(KafkaConsumerConfiguration config)
        {
            ConsumerConfig consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config.BootstrapServers,
                GroupId = $"{config.GroupId}{LTSConstants.Kafka.SNAPSHOT_CONSUMER_GROUP_SUFFIX}",
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Latest,
            };

            return new ConsumerBuilder<string, byte[]>(consumerConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.ByteArray)
                .Build();
        }
    }
}
