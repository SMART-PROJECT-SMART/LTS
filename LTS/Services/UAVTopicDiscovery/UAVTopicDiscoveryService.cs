using System.Collections.Concurrent;
using Confluent.Kafka;
using LTS.Common;
using LTS.Configuration;
using LTS.Services.UAVTopicDiscovery.Interfaces;
using Microsoft.Extensions.Options;

namespace LTS.Services.UAVTopicDiscovery
{
    public class UAVTopicDiscoveryService : IUAVTopicDiscoveryService, IHostedService
    {
        private readonly ConcurrentDictionary<int, bool> _cachedUAVIds;
        private readonly KafkaConsumerConfiguration _consumerConfig;
        private readonly ILogger<UAVTopicDiscoveryService> _logger;

        public UAVTopicDiscoveryService(
            IOptions<KafkaConsumerConfiguration> consumerConfig,
            ILogger<UAVTopicDiscoveryService> logger
        )
        {
            _cachedUAVIds = new ConcurrentDictionary<int, bool>();
            _consumerConfig = consumerConfig.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await DiscoverAndCacheUAVTopicsAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task DiscoverAndCacheUAVTopicsAsync()
        {
            using IAdminClient adminClient = new AdminClientBuilder(
                new AdminClientConfig { BootstrapServers = _consumerConfig.BootstrapServers }
            ).Build();

            Metadata metadata = adminClient.GetMetadata(
                TimeSpan.FromSeconds(LTSConstants.Kafka.METADATA_TIMEOUT_SECONDS)
            );

            foreach (TopicMetadata topic in metadata.Topics)
            {
                if (!topic.Topic.StartsWith(LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX))
                    continue;
                string tailIdString = topic.Topic.Substring(
                    LTSConstants.Kafka.UAV_DATA_TOPIC_PREFIX.Length
                );

                if (int.TryParse(tailIdString, out int tailId))
                {
                    _cachedUAVIds.TryAdd(tailId, true);
                }
            }

            await Task.CompletedTask;
        }

        public void AddUAVTopic(int tailId)
        {
            _cachedUAVIds.TryAdd(tailId, true);
        }

        public IEnumerable<int> GetAllCachedUAVIds()
        {
            return _cachedUAVIds.Keys;
        }
    }
}
