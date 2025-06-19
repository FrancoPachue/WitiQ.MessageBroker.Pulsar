using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WitiQ.MessageBroker.Pulsar.Core.Models;

namespace WitiQ.MessageBroker.Pulsar.Core.Abstractions;

public interface IWitiQPulsarConsumer<T> : IDisposable
{
    Task<WitiQPulsarMessage<T>> ReceiveAsync(CancellationToken cancellationToken = default);
    Task AcknowledgeAsync(WitiQPulsarMessage<T> message, CancellationToken cancellationToken = default);
    string Topic { get; }
    string SubscriptionName { get; }
    bool IsConnected { get; }
}