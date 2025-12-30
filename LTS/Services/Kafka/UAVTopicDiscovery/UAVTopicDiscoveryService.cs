using System.Collections.Concurrent;
using Confluent.Kafka;
using LTS.Common;
using LTS.Configuration;
using LTS.Services.Kafka.UAVTopicDiscovery.Interfaces;
using Microsoft.Extensions.Options;

namespace LTS.Services.UAVTopicDiscovery
{
    public class UAVTopicDiscoveryService : IUAVTopicDiscoveryService, IHostedService
    {
        private readonly ConcurrentDictionary<int, byte> _cachedUAVIds;
        private readonly KafkaConsumerConfiguration _consumerConfig;
        private readonly int _prefixLength;

        public UAVTopicDiscoveryService(IOptions<KafkaConsumerConfiguration> consumerConfig)
        {
            _cachedUAVIds = new ConcurrentDictionary<int, byte>();
            _consumerConfig = consumerConfig.Value;
            _prefixLength = LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX.Length;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            DiscoverAndCacheUAVTopics();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DiscoverAndCacheUAVTopicsAsync()
        {
            DiscoverAndCacheUAVTopics();
            return Task.CompletedTask;
        }

        private void DiscoverAndCacheUAVTopics()
        {
            using IAdminClient adminClient = new AdminClientBuilder(
                new AdminClientConfig { BootstrapServers = _consumerConfig.BootstrapServers }
            ).Build();

            Metadata metadata = adminClient.GetMetadata(
                TimeSpan.FromSeconds(LTSConstants.Kafka.METADATA_TIMEOUT_SECONDS)
            );

            foreach (TopicMetadata topicMetadata in metadata.Topics)
            {
                string topicName = topicMetadata.Topic;

                if (topicName.Length <= _prefixLength)
                {
                    continue;
                }

                if (
                    !topicName.StartsWith(
                        LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX,
                        StringComparison.Ordinal
                    )
                )
                {
                    continue;
                }

                ReadOnlySpan<char> tailIdSpan = topicName.AsSpan(_prefixLength);

                if (int.TryParse(tailIdSpan, out int tailId))
                {
                    _cachedUAVIds.TryAdd(tailId, 0);
                }
            }
        }

        public void AddUAVTopic(int tailId)
        {
            _cachedUAVIds.TryAdd(tailId, 0);
        }

        public IEnumerable<int> GetAllCachedUAVIds()
        {
            return _cachedUAVIds.Keys;
        }
    }
}
