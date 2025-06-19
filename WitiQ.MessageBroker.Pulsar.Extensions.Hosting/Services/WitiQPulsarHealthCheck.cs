using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;
using WitiQ.MessageBroker.Pulsar.Core.Models;

namespace WitiQ.MessageBroker.Pulsar.Extensions.Hosting
{
    /// <summary>
    /// Health check for WitiQ Pulsar client connectivity
    /// </summary>
    public class WitiQPulsarHealthCheck : IHealthCheck
    {
        private readonly IWitiQPulsarClient _client;
        private readonly ILogger<WitiQPulsarHealthCheck> _logger;

        public WitiQPulsarHealthCheck(
            IWitiQPulsarClient client,
            ILogger<WitiQPulsarHealthCheck> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if client is connected
                if (!_client.IsConnected)
                {
                    _logger.LogWarning("WitiQ Pulsar client is not connected");
                    return HealthCheckResult.Unhealthy("Pulsar client is not connected");
                }

                // Try to create a test producer to verify connectivity
                var testTopic = "health-check-topic";
                using var producer = await _client.CreateProducerAsync<string>(testTopic, null, cancellationToken);

                _logger.LogDebug("WitiQ Pulsar health check passed");
                return HealthCheckResult.Healthy("Pulsar client is connected and operational");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("WitiQ Pulsar health check was cancelled");
                return HealthCheckResult.Unhealthy("Health check was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WitiQ Pulsar health check failed");
                return HealthCheckResult.Unhealthy("Pulsar client health check failed", ex);
            }
        }
    }
}
