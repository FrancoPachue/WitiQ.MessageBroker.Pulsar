using System;
using System.Threading;
using System.Threading.Tasks;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;

namespace WitiQ.MessageBroker.Pulsar.Core.Abstractions;

public interface IWitiQPulsarClient : IDisposable
{
    Task<IWitiQPulsarProducer<T>> CreateProducerAsync<T>(string topic, ProducerConfiguration? config = null, CancellationToken cancellationToken = default);
    Task<IWitiQPulsarConsumer<T>> CreateConsumerAsync<T>(string topic, string subscriptionName, ConsumerConfiguration? config = null, CancellationToken cancellationToken = default);
    bool IsConnected { get; }
}