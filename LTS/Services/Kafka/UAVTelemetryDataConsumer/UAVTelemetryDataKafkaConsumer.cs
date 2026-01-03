using Confluent.Kafka;
using LTS.Common;
using LTS.Configuration;
using LTS.Services.Kafka.UAVTelemetryDataConsumer.Interfaces;
using Microsoft.Extensions.Options;

namespace LTS.Services.Kafka.UAVTelemetryDataConsumer
{
    public class UAVTelemetryDataKafkaConsumer : IUAVTelemetryDataKafkaConsumer
    {
        private readonly IConsumer<string, string> _kafkaConsumer;

        public UAVTelemetryDataKafkaConsumer(
            IOptions<KafkaConsumerConfiguration> kafkaConsumerConfiguration
        )
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaConsumerConfiguration.Value.BootstrapServers,
                GroupId = kafkaConsumerConfiguration.Value.GroupId,
                EnableAutoCommit = kafkaConsumerConfiguration.Value.EnableAutoCommit,
                AutoOffsetReset = AutoOffsetReset.Latest,
            };

            _kafkaConsumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();
        }

        public ConsumeResult<string, string> ConsumeUAVTelemetryData()
        {
            return _kafkaConsumer.Consume();
        }

        public void SubsribeToTopic(string tailId)
        {
            _kafkaConsumer.Subscribe($"{LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX}{tailId}");
        }

        public void Dispose()
        {
            _kafkaConsumer.Unsubscribe();
            _kafkaConsumer.Dispose();
        }
    }
}
