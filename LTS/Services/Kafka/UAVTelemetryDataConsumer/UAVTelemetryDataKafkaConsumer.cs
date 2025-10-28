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
        private readonly IWantedUAVFieldsManager _wantedUAVFieldsManager;

        public UAVTelemetryDataKafkaConsumer(IWantedUAVFieldsManager wantedUavFieldsManager, IOptions<KafkaConsumerConfiguration> kafkaConsumerConfiguration)
        {
            _wantedUAVFieldsManager = wantedUavFieldsManager;
            
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaConsumerConfiguration.Value.BootstrapServers,
                GroupId = kafkaConsumerConfiguration.Value.GroupId,
                EnableAutoCommit = kafkaConsumerConfiguration.Value.EnableAutoCommit,
                AutoOffsetReset = AutoOffsetReset.Earliest
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

        public void UpdateUAVTopicsToConsume()
        {
            _kafkaConsumer.Unsubscribe();
            IEnumerable<int> wantedUAVToConsume = _wantedUAVFieldsManager.GetAllWantedUAVs();
            List<string> wantedTopicToConsume = wantedUAVToConsume
                .Select(tailId => $"{LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX}{tailId}").ToList();
            _kafkaConsumer.Subscribe(wantedTopicToConsume);
        }
    }
}
