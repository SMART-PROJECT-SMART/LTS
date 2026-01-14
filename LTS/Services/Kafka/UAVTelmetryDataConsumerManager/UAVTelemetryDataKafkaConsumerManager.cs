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
        private readonly ConcurrentDictionary<string, int> _consumerTimeoutCounts;
        private readonly KafkaConsumerConfiguration _kafkaConfig;
        private int _pollCycleCount;

        private const int TIMEOUT_THRESHOLD = 3;
        private const int RETRY_CYCLE_INTERVAL = 10;

        public UAVTelemetryDataKafkaConsumerManager(
            IOptions<KafkaConsumerConfiguration> kafkaConfig
        )
        {
            _kafkaConfig = kafkaConfig.Value;
            _consumers = new ConcurrentDictionary<string, IUAVTelemetryDataKafkaConsumer>();
            _consumerTimeoutCounts = new ConcurrentDictionary<string, int>();
            _pollCycleCount = 0;
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
                _consumerTimeoutCounts.TryRemove(tailId, out _);
            }
        }

        public IEnumerable<ConsumeResult<string, string>> ConsumeUAVTelemetryData()
        {
            _pollCycleCount++;
            bool isRetryCycle = _pollCycleCount % RETRY_CYCLE_INTERVAL == 0;

            foreach (KeyValuePair<string, IUAVTelemetryDataKafkaConsumer> consumerEntry in _consumers)
            {
                string tailId = consumerEntry.Key;
                IUAVTelemetryDataKafkaConsumer consumer = consumerEntry.Value;

                int timeoutCount = _consumerTimeoutCounts.GetOrAdd(tailId, 0);

                if (!isRetryCycle && timeoutCount >= TIMEOUT_THRESHOLD)
                {
                    continue;
                }

                ConsumeResult<string, string> uavsTelmetryData = consumer.ConsumeUAVTelemetryData();

                if (uavsTelmetryData != null)
                {
                    _consumerTimeoutCounts[tailId] = 0;
                    yield return uavsTelmetryData;
                }
                else
                {
                    _consumerTimeoutCounts[tailId] = timeoutCount + 1;
                }
            }
        }
    }
}
