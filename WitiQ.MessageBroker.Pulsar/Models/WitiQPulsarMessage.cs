using System;
using System.Collections.Generic;

namespace WitiQ.MessageBroker.Pulsar.Core.Models;

public class WitiQPulsarMessage<T>
{
    public string MessageId { get; set; } = string.Empty;
    public T Data { get; set; } = default!;
    public string? Key { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
    public DateTime PublishTime { get; set; }
    public DateTime EventTime { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public int RedeliveryCount { get; set; }
}