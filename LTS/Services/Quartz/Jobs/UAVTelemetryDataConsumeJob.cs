using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Core.Common.Enums;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager;
using LTS.Services.UAVDataStorage.Interfaces;
using Quartz;
using static System.Int32;

namespace LTS.Services.Quartz.Jobs
{
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
                    byte[]
                > consumeResult in _uavTelemetryDataKafkaConsumerManager.ConsumeUAVTelemetryData()
            )
            {
                TryParse(consumeResult.Message.Key, out int tailId);

                IEnumerable<KeyValuePair<TelemetryFields, double>> telemetryData =
                    DeserializeTelemetryData(consumeResult.Message.Value);

                _uavTelemetryDataStorage.SaveUAVTelemetryData(tailId, telemetryData);
            }
            return Task.CompletedTask;
        }

        private IEnumerable<KeyValuePair<TelemetryFields, double>> DeserializeTelemetryData(
            byte[] data
        )
        {
            string json = System.Text.Encoding.UTF8.GetString(data);

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var telemetryDict = JsonSerializer.Deserialize<Dictionary<TelemetryFields, double>>(
                json,
                options
            )!;
            return telemetryDict;
        }
    }
}
