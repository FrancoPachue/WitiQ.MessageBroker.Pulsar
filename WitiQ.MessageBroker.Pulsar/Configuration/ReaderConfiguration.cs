using System;
using System.Collections.Generic;

namespace WitiQ.MessageBroker.Pulsar.Core.Configuration;

public class ReaderConfiguration
{
    public string? ReaderName { get; set; }
    public int ReceiverQueueSize { get; set; } = 1000;
    public bool ReadCompacted { get; set; } = false;
    public string? StartMessageId { get; set; }
    public DateTime? StartMessageFromRollbackDuration { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}
