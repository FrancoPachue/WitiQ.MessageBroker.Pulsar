using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;

public interface IWitiQPulsarConsumerServiceBuilder
{
    /// <summary>
    /// Adds a consumer service using a handler service
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <typeparam name="THandler">Handler service type</typeparam>
    /// <param name="topic">Topic name</param>
    /// <param name="subscriptionName">Subscription name</param>
    /// <param name="config">Consumer configuration (optional)</param>
    /// <returns>Builder for chaining</returns>
    IWitiQPulsarConsumerServiceBuilder AddConsumer<T, THandler>(
        string topic,
        string subscriptionName,
        ConsumerConfiguration? config = null)
        where THandler : class, IWitiQPulsarMessageHandler<T>;
}