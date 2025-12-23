using Confluent.Kafka;

namespace LTS.Services.Kafka.UAVTelmetryDataConsumerManager.Interfaces
{
    public interface IUAVTelemetryDataKafkaConsumerManager
    {
        public void AddConsumer(string tailId);
        public void RemoveConsumer(string tailId);
        public IEnumerable<ConsumeResult<string, byte[]>> ConsumeUAVTelemetryData();
    }
}
