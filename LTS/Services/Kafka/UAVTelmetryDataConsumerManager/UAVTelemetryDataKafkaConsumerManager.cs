using Confluent.Kafka;
using LTS.Services.Kafka.UAVTelemetryDataConsumer;
using Microsoft.Extensions.DependencyInjection;

namespace LTS.Services.Kafka.UAVTelmetryDataConsumerManager
{
    public class UAVTelemetryDataKafkaConsumerManager : IUAVTelemetryDataKafkaConsumerManager
    {
        private readonly Dictionary<string, IUAVTelemetryDataKafkaConsumer> _consumers;
        private readonly IServiceProvider _serviceProvider;

        public UAVTelemetryDataKafkaConsumerManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _consumers = new Dictionary<string, IUAVTelemetryDataKafkaConsumer>();
        }

        public void AddConsumer(string tailId)
        {
            IUAVTelemetryDataKafkaConsumer? newConsumer =
                _serviceProvider.GetService<IUAVTelemetryDataKafkaConsumer>();
            if (newConsumer == null)
                return;
            newConsumer.SubsribeToTopic(tailId);
            _consumers.Add(tailId, newConsumer);
        }

        public void RemoveConsumer(string tailId)
        {
            if (!_consumers.TryGetValue(tailId, out var consumerToRemove))
                return;
            consumerToRemove.Dispose();
            _consumers.Remove(tailId);
        }

        public IEnumerable<ConsumeResult<string, byte[]>> ConsumeUAVTelemetryData()
        {
            foreach (IUAVTelemetryDataKafkaConsumer consumer in _consumers.Values)
            {
                ConsumeResult<string, byte[]> uavsTelmetryData = consumer.ConsumeUAVTelemetryData();
                yield return uavsTelmetryData;
            }
        }
    }
}
