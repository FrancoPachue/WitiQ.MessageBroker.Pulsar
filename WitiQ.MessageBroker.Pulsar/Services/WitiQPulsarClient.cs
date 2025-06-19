using System;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;
using CompressionType = WitiQ.MessageBroker.Pulsar.Core.Configuration.CompressionType;
using SubscriptionType = WitiQ.MessageBroker.Pulsar.Core.Configuration.SubscriptionType;

namespace WitiQ.MessageBroker.Pulsar.Core.Services
{
    public class WitiQPulsarClient : IWitiQPulsarClient
    {
        private readonly IPulsarClient _pulsarClient;
        private readonly ILogger<WitiQPulsarClient> _logger;
        private readonly WitiQPulsarClientConfiguration _configuration;
        private bool _disposed = false;

        public WitiQPulsarClient(
            IOptions<WitiQPulsarClientConfiguration> configuration,
            ILogger<WitiQPulsarClient> logger)
        {
            _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _pulsarClient = CreatePulsarClient();
            _logger.LogInformation("WitiQ Pulsar client created for service URL: {ServiceUrl}", _configuration.ServiceUrl);
        }

        public bool IsConnected => true; 

        public Task<IWitiQPulsarProducer<T>> CreateProducerAsync<T>(
            string topic,
            ProducerConfiguration? config = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

            try
            {
                _logger.LogDebug("Creating producer for topic: {Topic}", topic);

                var producerOptions = new ProducerOptions<byte[]>(topic, Schema.ByteArray);

                // Aplicar configuración si existe
                if (config != null)
                {
                    if (!string.IsNullOrEmpty(config.ProducerName))
                        producerOptions.ProducerName = config.ProducerName;

                    producerOptions.CompressionType = MapCompressionType(config.CompressionType);
                }

                var producer = _pulsarClient.CreateProducer(producerOptions);

                _logger.LogInformation("Producer created successfully for topic: {Topic}", topic);

                return Task.FromResult<IWitiQPulsarProducer<T>>(
                    new WitiQPulsarProducer<T>(producer, topic, _logger));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create producer for topic: {Topic}", topic);
                throw;
            }
        }

        public Task<IWitiQPulsarConsumer<T>> CreateConsumerAsync<T>(
            string topic,
            string subscriptionName,
            ConsumerConfiguration? config = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

            if (string.IsNullOrWhiteSpace(subscriptionName))
                throw new ArgumentException("Subscription name cannot be null or empty", nameof(subscriptionName));

            try
            {
                _logger.LogDebug("Creating consumer for topic: {Topic}, subscription: {Subscription}", topic, subscriptionName);

                var consumerOptions = new ConsumerOptions<byte[]>(subscriptionName, topic, Schema.ByteArray);

                if (config != null)
                {
                    if (!string.IsNullOrEmpty(config.ConsumerName))
                        consumerOptions.ConsumerName = config.ConsumerName;

                    consumerOptions.SubscriptionType = MapSubscriptionType(config.SubscriptionType);

                    consumerOptions.InitialPosition = MapInitialPosition(config.InitialPosition);

                    if (config.ReadCompacted)
                        consumerOptions.ReadCompacted = config.ReadCompacted;
                }

                var consumer = _pulsarClient.CreateConsumer(consumerOptions);

                _logger.LogInformation("Consumer created successfully for topic: {Topic}, subscription: {Subscription}", topic, subscriptionName);

                return Task.FromResult<IWitiQPulsarConsumer<T>>(
                    new WitiQPulsarConsumer<T>(consumer, topic, subscriptionName, _logger));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create consumer for topic: {Topic}, subscription: {Subscription}", topic, subscriptionName);
                throw;
            }
        }

        private IPulsarClient CreatePulsarClient()
        {
            var clientBuilder = PulsarClient.Builder()
                .ServiceUrl(new Uri(_configuration.ServiceUrl));

            // Aplicar configuración de autenticación si existe
            if (_configuration.Authentication != null && !string.IsNullOrEmpty(_configuration.Authentication.Token))
            {
                clientBuilder.Authentication(AuthenticationFactory.Token(_configuration.Authentication.Token));
            }

            return clientBuilder.Build();
        }

        // Métodos de mapeo para configuraciones
        private static DotPulsar.CompressionType MapCompressionType(CompressionType compressionType)
        {
            return compressionType switch
            {
                CompressionType.None => DotPulsar.CompressionType.None,
                CompressionType.Lz4 => DotPulsar.CompressionType.Lz4,
                CompressionType.Zlib => DotPulsar.CompressionType.Zlib,
                CompressionType.Zstd => DotPulsar.CompressionType.Zstd,
                CompressionType.Snappy => DotPulsar.CompressionType.Snappy,
                _ => DotPulsar.CompressionType.None
            };
        }

        private static DotPulsar.SubscriptionType MapSubscriptionType(SubscriptionType subscriptionType)
        {
            return subscriptionType switch
            {
                SubscriptionType.Exclusive => DotPulsar.SubscriptionType.Exclusive,
                SubscriptionType.Shared => DotPulsar.SubscriptionType.Shared,
                SubscriptionType.Failover => DotPulsar.SubscriptionType.Failover,
                SubscriptionType.KeyShared => DotPulsar.SubscriptionType.KeyShared,
                _ => DotPulsar.SubscriptionType.Exclusive
            };
        }

        private static SubscriptionInitialPosition MapInitialPosition(InitialPosition initialPosition)
        {
            return initialPosition switch
            {
                InitialPosition.Latest => SubscriptionInitialPosition.Latest,
                InitialPosition.Earliest => SubscriptionInitialPosition.Earliest,
                _ => SubscriptionInitialPosition.Latest
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _pulsarClient?.DisposeAsync();
                    _logger.LogInformation("WitiQ Pulsar client disposed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing WitiQ Pulsar client");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }
}