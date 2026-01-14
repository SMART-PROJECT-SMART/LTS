using Confluent.Kafka;
using LTS.Common;
using LTS.Configuration;
using LTS.Services.Kafka.UAVTelemetryDataConsumer.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LTS.Services.Kafka.UAVTelemetryDataConsumer
{
    public class UAVTelemetryDataKafkaConsumer : IUAVTelemetryDataKafkaConsumer
    {
        private readonly IConsumer<string, string> _kafkaConsumer;
        private bool _isDisposed;
        public UAVTelemetryDataKafkaConsumer(
            KafkaConsumerConfiguration kafkaConsumerConfiguration,
            string tailId)
        {
            _isDisposed = false;
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaConsumerConfiguration.BootstrapServers,
                GroupId = $"{kafkaConsumerConfiguration.GroupIdPrefix}-tailId-{tailId}",
                AutoOffsetReset = AutoOffsetReset.Latest
            };

            _kafkaConsumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();

            _kafkaConsumer.Subscribe($"{LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX}{tailId}");
        }

        public ConsumeResult<string, string> ConsumeUAVTelemetryData()
        {
            return !_isDisposed ? _kafkaConsumer.Consume() : null;
        }

        public void Dispose()
        {
            _isDisposed = true;
            _kafkaConsumer.Unsubscribe();
            _kafkaConsumer.Dispose();
        }
    }
}
