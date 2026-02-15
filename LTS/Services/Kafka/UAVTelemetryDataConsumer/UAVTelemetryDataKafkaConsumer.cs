using Confluent.Kafka;
using LTS.Common;
using LTS.Configuration;
using LTS.Services.Kafka.UAVTelemetryDataConsumer.Interfaces;

namespace LTS.Services.Kafka.UAVTelemetryDataConsumer
{
    public class UAVTelemetryDataKafkaConsumer : IUAVTelemetryDataKafkaConsumer
    {
        private static readonly TimeSpan ConsumeTimeout = TimeSpan.FromMilliseconds(100);

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

            string topicName = $"{LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX}{tailId}";
            var partition = new Partition(LTSConstants.Kafka.CANONICAL_TELEMETRY_PARTITION);
            _kafkaConsumer.Assign(new[] { new TopicPartition(topicName, partition) });
        }

        public ConsumeResult<string, string>? ConsumeUAVTelemetryData()
        {
            if (_isDisposed)
            {
                return null;
            }

            try
            {
                return _kafkaConsumer.Consume(ConsumeTimeout);
            }
            catch (ConsumeException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            try
            {
                _kafkaConsumer.Unassign();
                _kafkaConsumer.Close();
                _kafkaConsumer.Dispose();
            }
            catch
            {
            }
        }
    }
}
