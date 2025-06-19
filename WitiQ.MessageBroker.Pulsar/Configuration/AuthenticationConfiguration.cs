using System.Collections.Generic;

namespace WitiQ.MessageBroker.Pulsar.Core.Configuration;

public class AuthenticationConfiguration
{
    public string Type { get; set; } = string.Empty; // "token", "jwt", "oauth2", etc.
    public string? Token { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}