using System;
using System.Text;
using System.Text.Json;

namespace WitiQ.MessageBroker.Pulsar.Core.Helpers;

internal static class MessageSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var json = JsonSerializer.Serialize(value, DefaultOptions);
        return Encoding.UTF8.GetBytes(json);
    }

    public static T Deserialize<T>(ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty)
            throw new ArgumentException("Data cannot be empty", nameof(data));

        var json = Encoding.UTF8.GetString(data.Span);
        var result = JsonSerializer.Deserialize<T>(json, DefaultOptions);

        return result ?? throw new InvalidOperationException($"Deserialization resulted in null for type {typeof(T).Name}");
    }
}