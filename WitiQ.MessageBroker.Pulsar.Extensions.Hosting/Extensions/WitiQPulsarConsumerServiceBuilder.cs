using Microsoft.Extensions.DependencyInjection;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;
using WitiQ.MessageBroker.Pulsar.Extensions.Hosting;

/// <summary>
/// Implementation of the consumer service builder
/// </summary>
internal class WitiQPulsarConsumerServiceBuilder : IWitiQPulsarConsumerServiceBuilder
{
    private readonly IServiceCollection _services;

    public WitiQPulsarConsumerServiceBuilder(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IWitiQPulsarConsumerServiceBuilder AddConsumer<T, THandler>(
        string topic,
        string subscriptionName,
        ConsumerConfiguration? config = null)
        where THandler : class, IWitiQPulsarMessageHandler<T>
    {
        _services.AddWitiQPulsarConsumerService<T, THandler>(topic, subscriptionName, config);
        return this;
    }
}