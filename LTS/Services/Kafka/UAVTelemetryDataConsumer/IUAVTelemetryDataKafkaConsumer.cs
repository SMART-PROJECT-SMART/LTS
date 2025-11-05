using Confluent.Kafka;

namespace LTS.Services.Kafka.UAVTelemetryDataConsumer
{
    public interface IUAVTelemetryDataKafkaConsumer : IDisposable
    {
        public ConsumeResult<string, byte[]> ConsumeUAVTelemetryData();
        public void SubsribeToTopic(string tailId);
    }
}
