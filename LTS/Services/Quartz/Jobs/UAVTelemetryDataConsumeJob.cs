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

        public UAVTelemetryDataConsumeJob(
            IUAVTelemetryDataKafkaConsumerManager uavTelemetryDataKafkaConsumerManager,
            IUAVTelemetryDataStorage uavTelemetryDataStorage
        )
        {
            _uavTelemetryDataKafkaConsumerManager = uavTelemetryDataKafkaConsumerManager;
            _uavTelemetryDataStorage = uavTelemetryDataStorage;
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

        private IEnumerable<KeyValuePair<TelemetryFields, double>> DeserializeTelemetryData(string json)
        {
            Dictionary<TelemetryFields, double> telemetryDict = JsonConvert.DeserializeObject<
                Dictionary<TelemetryFields, double>
            >(json, JsonSerializationSettings.TelemetrySettings)!;
            return telemetryDict;
        }
    }
}
