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
        private readonly IConsumer<string, byte[]> _kafkaConsumer;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IUAVTopicDiscoveryService _topicDiscoveryService;
        private readonly TimeSpan _consumeTimeout;

        public UAVSnapshotConsumer(
            IOptions<KafkaConsumerConfiguration> configuration,
            IUAVTopicDiscoveryService topicDiscoveryService
        )
        {
            _kafkaConsumer = CreateConsumer(configuration.Value);
            _jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true,
            };
            _topicDiscoveryService = topicDiscoveryService;
            _consumeTimeout = TimeSpan.FromSeconds(LTSConstants.Kafka.CONSUME_TIMEOUT_SECONDS);
        }

        public IEnumerable<UAVTelemetryDataDto> PeekAllUAVSnapshots()
        {
            IEnumerable<int> uavIds = _topicDiscoveryService.GetAllCachedUAVIds();
            List<UAVTelemetryDataDto> snapshots = new List<UAVTelemetryDataDto>();

            foreach (int id in uavIds)
            {
                UAVTelemetryDataDto? snapshot = FetchSnapshot(id);
                if (snapshot != null)
                {
                    snapshots.Add(snapshot);
                }
            }

            return snapshots;
        }

        public void Dispose()
        {
            _kafkaConsumer.Dispose();
        }

        private UAVTelemetryDataDto? FetchSnapshot(int uavId)
        {
            TopicPartition partition = CreatePartition(uavId);
            Offset? offset = QueryLatestOffset(partition);

            if (offset == null)
                return null;

            byte[]? payload = ConsumeAtOffset(partition, offset.Value);

            if (payload == null)
                return null;

            Dictionary<TelemetryFields, double> telemetry = ParseTelemetry(payload);
            return new UAVTelemetryDataDto(uavId, telemetry);
        }

        private TopicPartition CreatePartition(int uavId)
        {
            return new TopicPartition(
                $"{LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX}{uavId}",
                new Partition(LTSConstants.Kafka.DEFAULT_PARTITION)
            );
        }

        private Offset? QueryLatestOffset(TopicPartition partition)
        {
            WatermarkOffsets? watermarks = _kafkaConsumer.QueryWatermarkOffsets(
                partition,
                _consumeTimeout
            );

            if (watermarks.High.Value <= 0 || watermarks.High <= watermarks.Low)
            {
                return null;
            }

            return watermarks.High - 1;
        }

        private byte[]? ConsumeAtOffset(TopicPartition partition, Offset offset)
        {
            _kafkaConsumer.Assign([new TopicPartitionOffset(partition, offset)]);

            ConsumeResult<string, byte[]> result = _kafkaConsumer.Consume(_consumeTimeout);

            if (result == null || result.IsPartitionEOF)
            {
                return null;
            }

            return result.Message.Value;
        }

        private Dictionary<TelemetryFields, double> ParseTelemetry(byte[] payload)
        {
            if (payload.Length == 0)
                return new Dictionary<TelemetryFields, double>();

            return JsonSerializer.Deserialize<Dictionary<TelemetryFields, double>>(
                    payload,
                    _jsonOptions
                ) ?? new Dictionary<TelemetryFields, double>();
        }

        private static IConsumer<string, byte[]> CreateConsumer(KafkaConsumerConfiguration config)
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
