using Confluent.Kafka;

namespace LTS.Services.Kafka.UAVTelemetryDataConsumer
{
    public interface IUAVTelemetryDataKafkaConsumer
    {
        public ConsumeResult<string, byte[]> ConsumeUAVTelemetryData();
        public void UpdateUAVTopicsToConsume();
    }
}
