using Confluent.Kafka;
using Core.Common.Enums;
using LTS.Common;
using LTS.Configuration;
using LTS.Dto;
using LTS.Services.ActiveUAVFetcher.Interfaces;
using LTS.Services.Kafka.UAVSnapshotConsumer.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LTS.Services.Kafka.UAVSnapshotConsumer
{
    public class UAVSnapshotConsumer : IUAVSnapshotConsumer, IDisposable
    {
        private readonly IConsumer<string, string> _kafkaConsumer;
        private readonly IUAVFetcher _activeUAVFetcher;
        private readonly TimeSpan _consumeTimeout;

        public UAVSnapshotConsumer(
            IOptions<KafkaConsumerConfiguration> configuration,
            IUAVFetcher activeUAVFetcher
        )
        {
            _kafkaConsumer = CreateConsumer(configuration.Value);
            _activeUAVFetcher = activeUAVFetcher;
            _consumeTimeout = TimeSpan.FromSeconds(LTSConstants.Kafka.CONSUME_TIMEOUT_SECONDS);
        }

        public async Task<IEnumerable<UAVTelemetryDataDto>> PeekAllUAVSnapshots()
        {
            IEnumerable<SimulatorUAVDto> allUAVs = await _activeUAVFetcher.GetAllUAVsDataAsync();
            List<UAVTelemetryDataDto> snapshots = new List<UAVTelemetryDataDto>();

            foreach (SimulatorUAVDto uav in allUAVs)
            {
                UAVTelemetryDataDto snapshot = FetchSnapshotOrDefault(uav);
                snapshots.Add(snapshot);
            }

            return snapshots;
        }

        public void Dispose()
        {
            _kafkaConsumer.Dispose();
        }

        private UAVTelemetryDataDto FetchSnapshotOrDefault(SimulatorUAVDto uav)
        {
            TopicPartition partition = CreatePartition(uav.TailId);
            Offset? offset = QueryLatestOffset(partition);

            if (offset == null)
                return CreateDefaultTelemetryData(uav);

            string? payload = ConsumeAtOffset(partition, offset.Value);

            if (payload == null)
                return CreateDefaultTelemetryData(uav);

            Dictionary<TelemetryFields, double> telemetry = ParseTelemetry(payload);
            UAVType uavType = ExtractUAVType(telemetry);
            return new UAVTelemetryDataDto(uav.TailId, uavType, telemetry);
        }

        private UAVTelemetryDataDto CreateDefaultTelemetryData(SimulatorUAVDto uav)
        {
            Dictionary<TelemetryFields, double> defaultTelemetry = new Dictionary<TelemetryFields, double>();

            foreach (TelemetryFields field in Enum.GetValues<TelemetryFields>())
            {
                defaultTelemetry[field] = field switch
                {
                    TelemetryFields.Latitude => uav.BaseLocation.Latitude,
                    TelemetryFields.Longitude => uav.BaseLocation.Longitude,
                    TelemetryFields.Altitude => uav.BaseLocation.Altitude,
                    TelemetryFields.UAVTypeValue => (double)uav.PlatformType,
                    TelemetryFields.TailId => uav.TailId,
                    _ => 0.0
                };
            }

            return new UAVTelemetryDataDto(uav.TailId, uav.PlatformType, defaultTelemetry);
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
            try
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
            catch (KafkaException) {
                return null;
            }
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
