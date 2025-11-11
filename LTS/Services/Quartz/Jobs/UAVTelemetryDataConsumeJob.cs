using LTS.Services.Kafka.UAVTelmetryDataConsumerManager;
using LTS.Services.UAVDataStorage;
using Quartz;

namespace LTS.Services.Quartz.Jobs
{
    public class UAVTelemetryDataConsumeJob : IJob
    {
        private readonly IUAVTelemetryDataKafkaConsumerManager _uavTelemetryDataKafkaConsumerManager;
        private readonly IUAVTelemetryDataStorage _uavTelemetryDataStorage;
        public Task Execute(IJobExecutionContext context)
        {
            foreach (var uavTelemetryData in _uavTelemetryDataKafkaConsumerManager.ConsumeUAVTelemetryData())
            {
                _uavTelemetryDataStorage.StoreUAVTelemetryData(uavTelemetryData.Key, uavTelemetryData.Value);
            }
            return Task.CompletedTask;
        }
    }
}
