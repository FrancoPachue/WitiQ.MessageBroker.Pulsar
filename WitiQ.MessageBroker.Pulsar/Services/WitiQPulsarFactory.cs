using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;

namespace WitiQ.MessageBroker.Pulsar.Core.Services
{
    /// <summary>
    /// Factory implementation for creating Pulsar producers, consumers, and readers
    /// </summary>
    public class WitiQPulsarFactory : IWitiQPulsarFactory
    {
        private readonly IWitiQPulsarClient _client;
        private readonly ILogger<WitiQPulsarFactory> _logger;

        // ✅ CONSTRUCTOR SIMPLIFICADO - sin IOptionsMonitor
        public WitiQPulsarFactory(
            IWitiQPulsarClient client,
            ILogger<WitiQPulsarFactory> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IWitiQPulsarProducer<T>> CreateProducerAsync<T>(
            string topic,
            ProducerConfiguration? config = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating producer for topic: {Topic} via factory", topic);

            return await _client.CreateProducerAsync<T>(topic, config, cancellationToken);
        }

        public async Task<IWitiQPulsarConsumer<T>> CreateConsumerAsync<T>(
            string topic,
            string subscriptionName,
            ConsumerConfiguration? config = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating consumer for topic: {Topic}, subscription: {Subscription} via factory",
                topic, subscriptionName);

            return await _client.CreateConsumerAsync<T>(topic, subscriptionName, config, cancellationToken);
        }
    }
}
