public class PulsarOptions
{
    public string ServiceUrl { get; set; } = "pulsar://localhost:6650";
    public string Tenant { get; set; } = "public";
    public string Namespace { get; set; } = "default";

    public ProducerOptions ProducerDefaults { get; set; } = new();
    public ConsumerOptions ConsumerDefaults { get; set; } = new();

    public class ProducerOptions
    {
        public bool Batching { get; set; } = true;
        public string Compression { get; set; } = "LZ4";
        public int MaxPendingMessages { get; set; } = 1000;
    }

    public class ConsumerOptions
    {
        public string SubscriptionType { get; set; } = "Shared";
        public int AckTimeoutMs { get; set; } = 30000;
        public DeadLetterPolicyOptions DLQ { get; set; } = new();
    }

    public class DeadLetterPolicyOptions
    {
        public bool Enabled { get; set; } = true;
        public int MaxRetries { get; set; } = 3;
    }
}