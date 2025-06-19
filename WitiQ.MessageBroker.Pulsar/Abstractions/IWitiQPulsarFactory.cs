using System.Threading;
using System.Threading.Tasks;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;

namespace WitiQ.MessageBroker.Pulsar.Core.Abstractions
{
    /// <summary>
    /// Factory for creating Pulsar producers, consumers, and readers
    /// </summary>
    public interface IWitiQPulsarFactory
    {
        /// <summary>
        /// Creates a producer for the specified topic
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="topic">Topic name</param>
        /// <param name="config">Producer configuration (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Producer instance</returns>
        Task<IWitiQPulsarProducer<T>> CreateProducerAsync<T>(
            string topic,
            ProducerConfiguration? config = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a consumer for the specified topic and subscription
        /// </summary>
        /// <typeparam name="T">Message type</typeparam>
        /// <param name="topic">Topic name</param>
        /// <param name="subscriptionName">Subscription name</param>
        /// <param name="config">Consumer configuration (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Consumer instance</returns>
        Task<IWitiQPulsarConsumer<T>> CreateConsumerAsync<T>(
            string topic,
            string subscriptionName,
            ConsumerConfiguration? config = null,
            CancellationToken cancellationToken = default);
    }
}