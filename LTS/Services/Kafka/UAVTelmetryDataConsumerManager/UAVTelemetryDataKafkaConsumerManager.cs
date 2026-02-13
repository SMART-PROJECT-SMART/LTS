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
        private bool _isDisposed;

        public UAVTelemetryDataKafkaConsumerManager(IOptions<KafkaConsumerConfiguration> kafkaConfig)
        {
            _kafkaConfig = kafkaConfig.Value;
            _consumers = new ConcurrentDictionary<string, IUAVTelemetryDataKafkaConsumer>();
            _isDisposed = false;
        }

        public void AddConsumer(string tailId)
        {
            if (_isDisposed)
            {
                return;
            }

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
            if (_isDisposed)
            {
                yield break;
            }

            foreach (KeyValuePair<string, IUAVTelemetryDataKafkaConsumer> consumerEntry in _consumers)
            {
                IUAVTelemetryDataKafkaConsumer consumer = consumerEntry.Value;

                ConsumeResult<string, string>? uavsTelmetryData = consumer.ConsumeUAVTelemetryData();

                if (uavsTelmetryData != null)
                {
                    yield return uavsTelmetryData;
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            foreach (var consumer in _consumers.Values)
            {
                try
                {
                    consumer.Dispose();
                }
                catch
                {
                }
            }

            _consumers.Clear();
        }
    }
}
