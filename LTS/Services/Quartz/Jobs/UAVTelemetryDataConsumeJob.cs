using Confluent.Kafka;
using Core.Common.Enums;
using LTS.Common;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager.Interfaces;
using LTS.Services.UAVDataStorage.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using static System.Int32;

namespace LTS.Services.Quartz.Jobs
{
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

                _uavTelemetryDataStorage.SaveUAVTelemetryData(tailId, telemetryData);
            }
            return Task.CompletedTask;
        }

        private IEnumerable<KeyValuePair<TelemetryFields, double>> DeserializeTelemetryData(
            string json
        )
        {
            Dictionary<TelemetryFields, double> telemetryDict = JsonConvert.DeserializeObject<
                Dictionary<TelemetryFields, double>
            >(json, JsonSerializationSettings.TelemetrySettings)!;
            return telemetryDict;
        }
    }
}
