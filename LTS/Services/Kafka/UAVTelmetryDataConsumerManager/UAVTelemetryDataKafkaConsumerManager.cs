using System.Collections.Concurrent;
using Confluent.Kafka;
using LTS.Configuration;
using LTS.Services.Kafka.UAVTelemetryDataConsumer;
using LTS.Services.Kafka.UAVTelemetryDataConsumer.Interfaces;
using LTS.Services.Kafka.UAVTelmetryDataConsumerManager.Interfaces;
using Microsoft.Extensions.Options;

namespace LTS.Services.Kafka.UAVTelmetryDataConsumerManager
{
    public class UAVTelemetryDataKafkaConsumerManager : IUAVTelemetryDataKafkaConsumerManager
    {
        private readonly ConcurrentDictionary<string, IUAVTelemetryDataKafkaConsumer> _consumers;
        private readonly KafkaConsumerConfiguration _kafkaConfig;

        public UAVTelemetryDataKafkaConsumerManager(
            IOptions<KafkaConsumerConfiguration> kafkaConfig
        )
        {
            _kafkaConfig = kafkaConfig.Value;
            _consumers = new ConcurrentDictionary<string, IUAVTelemetryDataKafkaConsumer>();
        }

        public void AddConsumer(string tailId)
        {
            _consumers.GetOrAdd(tailId, key =>
            {
                IUAVTelemetryDataKafkaConsumer newConsumer =
                    new UAVTelemetryDataKafkaConsumer(_kafkaConfig, key);
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

        public IEnumerable<ConsumeResult<string, string>> ConsumeUAVTelemetryData()
        {
            foreach (IUAVTelemetryDataKafkaConsumer consumer in _consumers.Values)
            {
                ConsumeResult<string, string> uavsTelmetryData = consumer.ConsumeUAVTelemetryData();
                yield return uavsTelmetryData;
            }
        }
    }
}
