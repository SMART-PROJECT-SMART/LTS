using Confluent.Kafka;
using LTS.Common;
using LTS.Models;
using LTS.Services.SubscriptionManager;
using Microsoft.Extensions.Options;

namespace LTS.Services.Kafka.UAVTelemetryDataConsumer
{
    public class UAVTelemetryDataKafkaConsumer : IUAVTelemetryDataKafkaConsumer
    {
        private readonly IConsumer<string, byte[]> _kafkaConsumer;

        public UAVTelemetryDataKafkaConsumer(IOptions<KafkaConsumerConfiguration> kafkaConsumerConfiguration)
        {
            
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaConsumerConfiguration.Value.BootstrapServers,
                GroupId = kafkaConsumerConfiguration.Value.GroupId,
                EnableAutoCommit = kafkaConsumerConfiguration.Value.EnableAutoCommit,
                AutoOffsetReset = AutoOffsetReset.Latest
            };

            _kafkaConsumer = new ConsumerBuilder<string, byte[]>(consumerConfig)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.ByteArray)
                .Build();
        }

        public ConsumeResult<string, byte[]> ConsumeUAVTelemetryData()
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
