using System.Threading;
using System.Threading.Tasks;
using WitiQ.MessageBroker.Pulsar.Core.Models;

namespace WitiQ.MessageBroker.Pulsar.Core.Abstractions
{
    /// <summary>
    /// Interface for handling Pulsar messages
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    public interface IWitiQPulsarMessageHandler<T>
    {
        /// <summary>
        /// Handles a received message
        /// </summary>
        /// <param name="message">The received message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task HandleAsync(WitiQPulsarMessage<T> message, CancellationToken cancellationToken);
    }
}