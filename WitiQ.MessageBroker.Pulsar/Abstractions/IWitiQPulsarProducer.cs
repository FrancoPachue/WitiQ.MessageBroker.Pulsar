using WitiQ.MessageBroker.Pulsar.Core.Models;

namespace WitiQ.MessageBroker.Pulsar.Core.Abstractions;

public interface IWitiQPulsarProducer<T> : IDisposable
{
    Task<string> SendAsync(T message, CancellationToken cancellationToken = default);
    Task<string> SendAsync(T message, MessageMetadata metadata, CancellationToken cancellationToken = default);
    Task<string> SendAsync(T message, string key, CancellationToken cancellationToken = default);
    string Topic { get; }
    bool IsConnected { get; }
}