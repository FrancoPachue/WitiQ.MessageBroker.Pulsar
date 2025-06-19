using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;
using WitiQ.MessageBroker.Pulsar.Core.Models;

namespace WitiQ.MessageBroker.Pulsar.Extensions.Hosting.Services
{
    /// <summary>
    /// Background service for automatically consuming messages using handler services
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <typeparam name="THandler">Handler service type</typeparam>
    public class WitiQPulsarConsumerBackgroundService<T, THandler> : BackgroundService
        where THandler : class, IWitiQPulsarMessageHandler<T>
    {
        private readonly IWitiQPulsarFactory _factory;
        private readonly string _topic;
        private readonly string _subscriptionName;
        private readonly ConsumerConfiguration? _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WitiQPulsarConsumerBackgroundService<T, THandler>> _logger;
        private IWitiQPulsarConsumer<T>? _consumer;

        public WitiQPulsarConsumerBackgroundService(
            IWitiQPulsarFactory factory,
            string topic,
            string subscriptionName,
            IServiceProvider serviceProvider,
            ConsumerConfiguration? config = null,
            ILogger<WitiQPulsarConsumerBackgroundService<T, THandler>>? logger = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
            _subscriptionName = subscriptionName ?? throw new ArgumentNullException(nameof(subscriptionName));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _config = config;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting WitiQ Pulsar consumer background service with handler {HandlerType} for topic: {Topic}, subscription: {Subscription}",
                typeof(THandler).Name, _topic, _subscriptionName);

            try
            {
                _consumer = await _factory.CreateConsumerAsync<T>(_topic, _subscriptionName, _config, stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var message = await _consumer.ReceiveAsync(stoppingToken);

                        _logger.LogDebug("Received message {MessageId} from topic {Topic}",
                            message.MessageId, _topic);

                        await ProcessMessage(message, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Consumer background service cancellation requested");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error receiving message from topic {Topic}, subscription {Subscription}",
                            _topic, _subscriptionName);

                        // Wait before retrying to avoid tight error loops
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in consumer background service for topic {Topic}, subscription {Subscription}",
                    _topic, _subscriptionName);
                throw;
            }
            finally
            {
                _logger.LogInformation("WitiQ Pulsar consumer background service with handler stopped for topic: {Topic}, subscription: {Subscription}",
                    _topic, _subscriptionName);
            }
        }

        private async Task ProcessMessage(WitiQPulsarMessage<T> message, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ Crear scope y resolver el handler
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();

                await handler.HandleAsync(message, cancellationToken);

                if (_consumer != null)
                {
                    await _consumer.AcknowledgeAsync(message, cancellationToken);
                    _logger.LogDebug("Message {MessageId} processed and acknowledged successfully via handler {HandlerType}",
                        message.MessageId, typeof(THandler).Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId} from topic {Topic} with handler {HandlerType}",
                    message.MessageId, _topic, typeof(THandler).Name);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping WitiQ Pulsar consumer background service with handler for topic: {Topic}, subscription: {Subscription}",
                _topic, _subscriptionName);

            await base.StopAsync(cancellationToken);

            if (_consumer != null)
            {
                _consumer.Dispose();
                _consumer = null;
            }
        }
    }
}
