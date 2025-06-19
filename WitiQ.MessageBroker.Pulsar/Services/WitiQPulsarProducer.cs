using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Microsoft.Extensions.Logging;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Helpers;
using WitiQ.MessageBroker.Pulsar.Core.Models;

namespace WitiQ.MessageBroker.Pulsar.Core.Services
{
    internal class WitiQPulsarProducer<T> : IWitiQPulsarProducer<T>
    {
        private readonly IProducer<byte[]> _producer;
        private readonly ILogger _logger;
        private bool _disposed = false;

        public WitiQPulsarProducer(IProducer<byte[]> producer, string topic, ILogger logger)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        }

        public string Topic { get; }
        public bool IsConnected => true;

        public async Task<string> SendAsync(T message, CancellationToken cancellationToken = default)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                _logger.LogDebug("Sending message to topic: {Topic}", Topic);

                var serializedMessage = MessageSerializer.Serialize(message);
                var messageId = await _producer.Send(serializedMessage.ToArray(), cancellationToken);

                _logger.LogDebug("Message sent successfully to topic: {Topic}, MessageId: {MessageId}", Topic, messageId);

                return messageId.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to topic: {Topic}", Topic);
                throw;
            }
        }

        public async Task<string> SendAsync(T message, MessageMetadata metadata, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            try
            {
                _logger.LogDebug("Sending message with metadata to topic: {Topic}", Topic);

                var serializedMessage = MessageSerializer.Serialize(message);
                var messageMetadata = new DotPulsar.MessageMetadata();

                if (!string.IsNullOrEmpty(metadata.Key))
                    messageMetadata.Key = metadata.Key;

                if (metadata.EventTime.HasValue)
                    messageMetadata.EventTime = metadata.EventTime.Value.ToUnixTimeMilliseconds();

                foreach (var property in metadata.Properties)
                {
                    messageMetadata[property.Key] = property.Value;
                }

                var messageId = await _producer.Send(messageMetadata, serializedMessage.ToArray(), cancellationToken);

                _logger.LogDebug("Message with metadata sent successfully to topic: {Topic}, MessageId: {MessageId}", Topic, messageId);

                return messageId.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message with metadata to topic: {Topic}", Topic);
                throw;
            }
        }

        public async Task<string> SendAsync(T message, string key, CancellationToken cancellationToken = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            try
            {
                _logger.LogDebug("Sending message with key to topic: {Topic}, Key: {Key}", Topic, key);

                var serializedMessage = MessageSerializer.Serialize(message);
                var messageMetadata = new DotPulsar.MessageMetadata();

                if (!string.IsNullOrEmpty(key))
                    messageMetadata.Key = key;

                var messageId = await _producer.Send(messageMetadata, serializedMessage.ToArray(), cancellationToken);

                _logger.LogDebug("Message with key sent successfully to topic: {Topic}, Key: {Key}, MessageId: {MessageId}", Topic, key, messageId);

                return messageId.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message with key to topic: {Topic}, Key: {Key}", Topic, key);
                throw;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _producer?.DisposeAsync();
                    _logger.LogDebug("WitiQ Pulsar producer disposed for topic: {Topic}", Topic);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing WitiQ Pulsar producer for topic: {Topic}", Topic);
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }
}
