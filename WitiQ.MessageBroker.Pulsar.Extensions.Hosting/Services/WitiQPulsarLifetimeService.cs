using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;

namespace WitiQ.MessageBroker.Pulsar.Extensions.Hosting.Services;

/// <summary>
/// Service that manages WitiQ Pulsar client lifetime
/// </summary>
public class WitiQPulsarLifetimeService : IHostedService
{
    private readonly IWitiQPulsarClient _client;
    private readonly ILogger<WitiQPulsarLifetimeService> _logger;

    public WitiQPulsarLifetimeService(
        IWitiQPulsarClient client,
        ILogger<WitiQPulsarLifetimeService> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WitiQ Pulsar client started successfully");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping WitiQ Pulsar client");

        try
        {
            _client.Dispose();
            _logger.LogInformation("WitiQ Pulsar client stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping WitiQ Pulsar client");
        }

        return Task.CompletedTask;
    }
}