namespace LTS.Configuration
{
    public class KafkaConsumerConfiguration
    {
        public string BootstrapServers { get; set; }
        public string GroupIdPrefix { get; set; }
        public string TopicPrefix { get; set; }
        public int SubscriptionCheckIntervalMs { get; set; }
        public string AutoOffsetReset { get; set; }
    }
}
