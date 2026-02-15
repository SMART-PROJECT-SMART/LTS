using Confluent.Kafka;
using Core.Common.Enums;
using LTS.Common;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager.Interfaces;
using LTS.Services.UAVDataStorage.Interfaces;
using Newtonsoft.Json;
using Quartz;
using static System.Int32;

namespace LTS.Services.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class UAVTelemetryDataConsumeJob : IJob
    {
        private readonly IUAVTelemetryDataKafkaConsumerManager _uavTelemetryDataKafkaConsumerManager;
        private readonly IUAVTelemetryDataStorage _uavTelemetryDataStorage;
        private readonly ILogger<UAVTelemetryDataConsumeJob> _logger;

        public UAVTelemetryDataConsumeJob(
            IUAVTelemetryDataKafkaConsumerManager uavTelemetryDataKafkaConsumerManager,
            IUAVTelemetryDataStorage uavTelemetryDataStorage,
            ILogger<UAVTelemetryDataConsumeJob> logger
        )
        {
            _uavTelemetryDataKafkaConsumerManager = uavTelemetryDataKafkaConsumerManager;
            _uavTelemetryDataStorage = uavTelemetryDataStorage;
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            int savedCount = 0;
            foreach (
                ConsumeResult<
                    string,
                    string
                > consumeResult in _uavTelemetryDataKafkaConsumerManager.ConsumeUAVTelemetryData()
            )
            {
                if (consumeResult == null || consumeResult.Message == null)
                {
                    continue;
                }

                if (!TryParse(consumeResult.Message.Key, out int tailId))
                {
                    continue;
                }

                IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData =
                    DeserializeTelemetryData(consumeResult.Message.Value);
                var data = telemetryData.ToList();

                _uavTelemetryDataStorage.SaveUAVTelemetryData(tailId, data);
                savedCount++;

                double lat = data.FirstOrDefault(k => k.Key == TelemetryFields.Latitude).Value;
                double lon = data.FirstOrDefault(k => k.Key == TelemetryFields.Longitude).Value;
                int partition = consumeResult.TopicPartition.Partition.Value;
                _logger.LogInformation(
                    "[Consume] saved tailId={TailId} partition={Partition} lat={Lat:F6} lon={Lon:F6}",
                    tailId,
                    partition,
                    lat,
                    lon
                );
            }

            _logger.LogInformation("[Consume] run finished savedCount={SavedCount}", savedCount);
            if (savedCount == 0)
            {
                _logger.LogDebug("[Consume] no messages this run");
            }

            return Task.CompletedTask;
        }

        private IEnumerable<KeyValuePair<TelemetryFields, double>> DeserializeTelemetryData(string json)
        {
            Dictionary<TelemetryFields, double> telemetryDict = JsonConvert.DeserializeObject<
                Dictionary<TelemetryFields, double>
            >(json, JsonSerializationSettings.TelemetrySettings)!;
            return telemetryDict;
        }
    }
}
