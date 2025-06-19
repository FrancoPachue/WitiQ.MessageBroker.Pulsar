using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;
using WitiQ.MessageBroker.Pulsar.Core.Models;
using WitiQ.MessageBroker.Pulsar.Extensions.Hosting.Services;

namespace WitiQ.MessageBroker.Pulsar.Extensions.Hosting;

/// <summary>
/// Extension methods for hosting WitiQ Pulsar consumers as background services
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    /// Adds a background service that consumes messages using a handler service
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    /// <typeparam name="THandler">Handler service type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="topic">Topic name</param>
    /// <param name="subscriptionName">Subscription name</param>
    /// <param name="config">Consumer configuration (optional)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsarConsumerService<T, THandler>(
        this IServiceCollection services,
        string topic,
        string subscriptionName,
        ConsumerConfiguration? config = null)
        where THandler : class, IWitiQPulsarMessageHandler<T>
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty", nameof(topic));

        if (string.IsNullOrWhiteSpace(subscriptionName))
            throw new ArgumentException("Subscription name cannot be null or empty", nameof(subscriptionName));

        // Register the handler service
        services.AddScoped<THandler>();

        // Register the background service
        services.AddHostedService<WitiQPulsarConsumerBackgroundService<T, THandler>>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<IWitiQPulsarFactory>();
            var logger = serviceProvider.GetRequiredService<ILogger<WitiQPulsarConsumerBackgroundService<T, THandler>>>();

            return new WitiQPulsarConsumerBackgroundService<T, THandler>(
                factory, topic, subscriptionName, serviceProvider, config, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds multiple consumer services for different topics with handlers
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Action to configure consumers</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsarConsumerServices(
        this IServiceCollection services,
        Action<IWitiQPulsarConsumerServiceBuilder> configure)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var builder = new WitiQPulsarConsumerServiceBuilder(services);
        configure(builder);

        return services;
    }

    /// <summary>
    /// Adds WitiQ Pulsar health check
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="name">Health check name (default: "witiq-pulsar")</param>
    /// <param name="failureStatus">Status to report on failure (default: Unhealthy)</param>
    /// <param name="tags">Tags for the health check</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsarHealthCheck(
        this IServiceCollection services,
        string name = "witiq-pulsar",
        HealthStatus? failureStatus = null,
        string[]? tags = null)
    {
        services.AddHealthChecks()
            .AddCheck<WitiQPulsarHealthCheck>(
                name,
                failureStatus ?? HealthStatus.Unhealthy,
                tags);

        return services;
    }

    /// <summary>
    /// Adds WitiQ Pulsar lifecycle management
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsarLifetimeManagement(this IServiceCollection services)
    {
        services.AddHostedService<Services.WitiQPulsarLifetimeService>();
        return services;
    }
}
