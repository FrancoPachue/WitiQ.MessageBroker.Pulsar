using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Helpers;
using WitiQ.MessageBroker.Pulsar.Core.Models;

namespace WitiQ.MessageBroker.Pulsar.Core.Services
{
    internal class WitiQPulsarConsumer<T> : IWitiQPulsarConsumer<T>
    {
        private readonly IConsumer<byte[]> _consumer;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IMessage<byte[]>> _messageCache;
        private bool _disposed = false;

        public WitiQPulsarConsumer(IConsumer<byte[]> consumer, string topic, string subscriptionName, ILogger logger)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            SubscriptionName = subscriptionName ?? throw new ArgumentNullException(nameof(subscriptionName));
            _messageCache = new ConcurrentDictionary<string, IMessage<byte[]>>();
        }

        public string Topic { get; }
        public string SubscriptionName { get; }
        public bool IsConnected => true; // Simplificado

        public async Task<WitiQPulsarMessage<T>> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Receiving message from topic: {Topic}, subscription: {Subscription}", Topic, SubscriptionName);

                var originalMessage = await _consumer.Receive(cancellationToken);
                var witiQMessage = MapToWitiQMessage(originalMessage);

                // Guardar referencia al mensaje original para acknowledgment
                _messageCache.TryAdd(witiQMessage.MessageId, originalMessage);

                _logger.LogDebug("Message received from topic: {Topic}, subscription: {Subscription}, MessageId: {MessageId}",
                    Topic, SubscriptionName, witiQMessage.MessageId);

                return witiQMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to receive message from topic: {Topic}, subscription: {Subscription}", Topic, SubscriptionName);
                throw;
            }
        }

        public async Task AcknowledgeAsync(WitiQPulsarMessage<T> message, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            try
            {
                _logger.LogDebug("Acknowledging message from topic: {Topic}, subscription: {Subscription}, MessageId: {MessageId}",
                    Topic, SubscriptionName, message.MessageId);

                if (_messageCache.TryRemove(message.MessageId, out var originalMessage))
                {
                    await _consumer.Acknowledge(originalMessage, cancellationToken);
                    _logger.LogDebug("Message acknowledged successfully from topic: {Topic}, subscription: {Subscription}, MessageId: {MessageId}",
                        Topic, SubscriptionName, message.MessageId);
                }
                else
                {
                    _logger.LogWarning("Could not find original message for acknowledgment: {MessageId}", message.MessageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acknowledge message from topic: {Topic}, subscription: {Subscription}, MessageId: {MessageId}",
                    Topic, SubscriptionName, message.MessageId);
                throw;
            }
        }

        public async Task NegativeAcknowledgeAsync(WitiQPulsarMessage<T> message, CancellationToken cancellationToken = default)
        {
            // MÉTODO STUB - NegativeAcknowledge no está disponible en la versión actual de DotPulsar
            _logger.LogWarning("NegativeAcknowledge not implemented yet in DotPulsar. MessageId: {MessageId}", message?.MessageId);
            await Task.CompletedTask;
        }

        private static WitiQPulsarMessage<T> MapToWitiQMessage(IMessage<byte[]> message)
        {
            return new WitiQPulsarMessage<T>
            {
                MessageId = message.MessageId.ToString(),
                Data = MessageSerializer.Deserialize<T>(message.Value()),
                Key = message.Key,
                Properties = message.Properties.ToDictionary(p => p.Key, p => p.Value),
                PublishTime = message.PublishTime.FromUnixTimeMilliseconds(),
                EventTime = message.EventTime.FromUnixTimeMilliseconds(),
                ProducerName = message.ProducerName ?? string.Empty,
                RedeliveryCount = (int)message.RedeliveryCount
            };
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _messageCache.Clear();
                    _consumer?.DisposeAsync();
                    _logger.LogDebug("WitiQ Pulsar consumer disposed for topic: {Topic}, subscription: {Subscription}", Topic, SubscriptionName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing WitiQ Pulsar consumer for topic: {Topic}, subscription: {Subscription}", Topic, SubscriptionName);
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }
}