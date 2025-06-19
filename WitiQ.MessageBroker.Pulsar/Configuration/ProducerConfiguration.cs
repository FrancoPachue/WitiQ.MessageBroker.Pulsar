using DotPulsar;
using System;
using System.Collections.Generic;

namespace WitiQ.MessageBroker.Pulsar.Core.Configuration;

public class ProducerConfiguration
{
    public string? ProducerName { get; set; }
    public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool BlockIfQueueFull { get; set; } = false;
    public int MaxPendingMessages { get; set; } = 1000;
    public CompressionType CompressionType { get; set; } = CompressionType.None;
    public bool BatchingEnabled { get; set; } = true;
    public int BatchingMaxMessages { get; set; } = 1000;
    public TimeSpan BatchingMaxDelay { get; set; } = TimeSpan.FromMilliseconds(10);
    public HashingScheme HashingScheme { get; set; } = HashingScheme.JavaStringHash;
    public Dictionary<string, string> Properties { get; set; } = new();
}