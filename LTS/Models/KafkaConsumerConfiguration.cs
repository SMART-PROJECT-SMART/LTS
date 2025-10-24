namespace LTS.Models
{
    public class KafkaConsumerConfiguration
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public string TopicPrefix { get; set; }
        public int SubscriptionCheckIntervalMs { get; set; }
        public bool EnableAutoCommit { get; set; }
        public string AutoOffsetReset { get; set; }
    }
}