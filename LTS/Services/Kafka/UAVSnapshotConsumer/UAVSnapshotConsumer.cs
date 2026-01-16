using Confluent.Kafka;
using Core.Common.Enums;
using LTS.Common;
using LTS.Configuration;
using LTS.Dto;
using LTS.Services.Kafka.UAVSnapshotConsumer.Interfaces;
using LTS.Services.Kafka.UAVTopicDiscovery.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LTS.Services.Kafka.UAVSnapshotConsumer
{
    public class UAVSnapshotConsumer : IUAVSnapshotConsumer, IDisposable
    {
        private readonly IConsumer<string, string> _kafkaConsumer;
        private readonly IUAVTopicDiscoveryService _topicDiscoveryService;
        private readonly TimeSpan _consumeTimeout;

        public UAVSnapshotConsumer(
            IOptions<KafkaConsumerConfiguration> configuration,
            IUAVTopicDiscoveryService topicDiscoveryService
        )
        {
            _kafkaConsumer = CreateConsumer(configuration.Value);
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

            string? payload = ConsumeAtOffset(partition, offset.Value);

            if (payload == null)
                return null;

            Dictionary<TelemetryFields, double> telemetry = ParseTelemetry(payload);
            UAVType uavType = ExtractUAVType(telemetry);
            return new UAVTelemetryDataDto(uavId, uavType, telemetry);
        }

        private static UAVType ExtractUAVType(Dictionary<TelemetryFields, double> telemetry)
        {
            if (!telemetry.TryGetValue(TelemetryFields.UAVTypeValue, out double typeValue))
            {
                throw new InvalidOperationException(
                    "UAVTypeValue is missing from telemetry data. Ensure the simulation is sending UAVType."
                );
            }

            return (UAVType)(int)typeValue;
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

            if (watermarks.High.Value <= LTSConstants.Kafka.EMPTY_TOPIC_OFFSET || watermarks.High <= watermarks.Low)
            {
                return null;
            }

            return watermarks.High - LTSConstants.Kafka.LAST_MESSAGE_OFFSET_ADJUSTMENT;
        }

        private string? ConsumeAtOffset(TopicPartition partition, Offset offset)
        {
            _kafkaConsumer.Assign([new TopicPartitionOffset(partition, offset)]);

            ConsumeResult<string, string> result = _kafkaConsumer.Consume(_consumeTimeout);

            _kafkaConsumer.Unassign();

            if (result == null || result.IsPartitionEOF)
            {
                return null;
            }

            return result.Message.Value;
        }

        private Dictionary<TelemetryFields, double> ParseTelemetry(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<TelemetryFields, double>();

            return JsonConvert.DeserializeObject<Dictionary<TelemetryFields, double>>(
                    json,
                    JsonSerializationSettings.TelemetrySettings
                ) ?? new Dictionary<TelemetryFields, double>();
        }

        private static IConsumer<string, string> CreateConsumer(KafkaConsumerConfiguration config)
        {
            ConsumerConfig consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config.BootstrapServers,
                GroupId = $"{config.GroupIdPrefix}{LTSConstants.Kafka.SNAPSHOT_CONSUMER_GROUP_SUFFIX}",
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Latest,
            };

            return new ConsumerBuilder<string, string>(consumerConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();
        }
    }
}
