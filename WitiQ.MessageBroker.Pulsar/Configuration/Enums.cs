namespace WitiQ.MessageBroker.Pulsar.Core.Configuration;

public enum CompressionType
{
    None,
    Lz4,
    Zlib,
    Zstd,
    Snappy
}

public enum HashingScheme
{
    JavaStringHash,
    Murmur3_32Hash
}

public enum SubscriptionType
{
    Exclusive,
    Shared,
    Failover,
    KeyShared
}

public enum InitialPosition
{
    Latest,
    Earliest
}