using Confluent.Kafka;

namespace LTS.Services.Kafka.UAVTelemetryDataConsumer.Interfaces
{
    public interface IUAVTelemetryDataKafkaConsumer : IDisposable
    {
        public ConsumeResult<string, byte[]> ConsumeUAVTelemetryData();
        public void SubsribeToTopic(string tailId);
    }
}
