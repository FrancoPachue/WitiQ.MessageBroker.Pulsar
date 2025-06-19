namespace WitiQ.MessageBroker.Pulsar.Core.Configuration;

public class WitiQPulsarClientConfiguration
{
    public string ServiceUrl { get; set; } = "pulsar://localhost:6650";
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public int MaxConcurrentLookupRequests { get; set; } = 5000;
    public int MaxLookupRedirects { get; set; } = 20;
    public bool UseTls { get; set; } = false;
    public string? TlsCertificatePath { get; set; }
    public string? TlsKeyPath { get; set; }
    public AuthenticationConfiguration? Authentication { get; set; }
    public Dictionary<string, string> Properties { get; set; } = new();
}