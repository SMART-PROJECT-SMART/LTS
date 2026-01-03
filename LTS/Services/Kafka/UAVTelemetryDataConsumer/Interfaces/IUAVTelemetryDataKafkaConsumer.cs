using Confluent.Kafka;

namespace LTS.Services.Kafka.UAVTelemetryDataConsumer.Interfaces
{
    public interface IUAVTelemetryDataKafkaConsumer : IDisposable
    {
        public ConsumeResult<string, string> ConsumeUAVTelemetryData();
        public void SubsribeToTopic(string tailId);
    }
}
