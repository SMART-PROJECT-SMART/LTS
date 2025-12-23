using System.Collections.Concurrent;
using Confluent.Kafka;
using LTS.Services.Kafka.UAVTelemetryDataConsumer.Interfaces;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LTS.Services.Kafka.UAVTelmetryDataConsumerManager
{
    public class UAVTelemetryDataKafkaConsumerManager : IUAVTelemetryDataKafkaConsumerManager
    {
        private readonly ConcurrentDictionary<string, IUAVTelemetryDataKafkaConsumer> _consumers;
        private readonly IServiceProvider _serviceProvider;

        public UAVTelemetryDataKafkaConsumerManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _consumers = new ConcurrentDictionary<string, IUAVTelemetryDataKafkaConsumer>();
        }

        public void AddConsumer(string tailId)
        {
            _consumers.GetOrAdd(tailId, key =>
            {
                IUAVTelemetryDataKafkaConsumer? newConsumer =
                    _serviceProvider.GetService<IUAVTelemetryDataKafkaConsumer>();
                if (newConsumer == null)
                    throw new InvalidOperationException("Failed to create UAV telemetry data consumer");
                newConsumer.SubsribeToTopic(key);
                return newConsumer;
            });
        }

        public void RemoveConsumer(string tailId)
        {
            if (_consumers.TryRemove(tailId, out IUAVTelemetryDataKafkaConsumer? consumerToRemove))
            {
                consumerToRemove.Dispose();
            }
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
