using DotPulsar;
using System;
using System.Collections.Generic;

namespace WitiQ.MessageBroker.Pulsar.Core.Configuration;

public class ConsumerConfiguration
{
    public string? ConsumerName { get; set; }
    public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Exclusive;
    public int ReceiverQueueSize { get; set; } = 1000;
    public TimeSpan AcknowledgmentGroupTime { get; set; } = TimeSpan.FromMilliseconds(100);
    public TimeSpan NegativeAckRedeliveryDelay { get; set; } = TimeSpan.FromMinutes(1);
    public int MaxTotalReceiverQueueSizeAcrossPartitions { get; set; } = 50000;
    public bool ReadCompacted { get; set; } = false;
    public InitialPosition InitialPosition { get; set; } = InitialPosition.Latest;
    public bool BatchReceivePolicy { get; set; } = false;
    public int BatchReceiveMaxMessages { get; set; } = 100;
    public TimeSpan BatchReceiveTimeout { get; set; } = TimeSpan.FromMilliseconds(100);
    public Dictionary<string, string> Properties { get; set; } = new();
}