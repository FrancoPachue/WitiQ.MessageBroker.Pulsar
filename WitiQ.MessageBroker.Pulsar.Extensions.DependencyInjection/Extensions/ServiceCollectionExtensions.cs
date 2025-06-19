
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;
using WitiQ.MessageBroker.Pulsar.Core.Services;
using System;

namespace WitiQ.MessageBroker.Pulsar.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring WitiQ Pulsar services in dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds WitiQ Pulsar services with default configuration (pulsar://localhost:6650)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsar(this IServiceCollection services)
    {
        return services.AddWitiQPulsar(options =>
        {
            options.ServiceUrl = "pulsar://localhost:6650";
        });
    }

    /// <summary>
    /// Adds WitiQ Pulsar services with service URL
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="serviceUrl">Pulsar service URL (e.g., "pulsar://localhost:6650")</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsar(this IServiceCollection services, string serviceUrl)
    {
        if (string.IsNullOrWhiteSpace(serviceUrl))
            throw new ArgumentException("Service URL cannot be null or empty", nameof(serviceUrl));

        return services.AddWitiQPulsar(options =>
        {
            options.ServiceUrl = serviceUrl;
        });
    }

    /// <summary>
    /// Adds WitiQ Pulsar services with configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsar(
        this IServiceCollection services,
        Action<WitiQPulsarClientConfiguration> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // Configure options
        services.Configure(configureOptions);

        // Register core services
        services.TryAddSingleton<IWitiQPulsarClient, WitiQPulsarClient>();

        // Register factory for creating producers/consumers on demand
        services.TryAddSingleton<IWitiQPulsarFactory, WitiQPulsarFactory>();

        return services;
    }

    /// <summary>
    /// Adds WitiQ Pulsar services with configuration from IConfiguration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="sectionName">Configuration section name (default: "WitiQPulsar")</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsar(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "WitiQPulsar")
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        services.Configure<WitiQPulsarClientConfiguration>(
            configuration.GetSection(sectionName));

        services.TryAddSingleton<IWitiQPulsarClient, WitiQPulsarClient>();
        services.TryAddSingleton<IWitiQPulsarFactory, WitiQPulsarFactory>();

        return services;
    }

    /// <summary>
    /// Adds WitiQ Pulsar services with detailed configuration options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="serviceUrl">Pulsar service URL</param>
    /// <param name="configureClient">Client configuration action</param>
    /// <param name="configureProducer">Default producer configuration action</param>
    /// <param name="configureConsumer">Default consumer configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWitiQPulsar(
        this IServiceCollection services,
        string serviceUrl,
        Action<WitiQPulsarClientConfiguration>? configureClient = null,
        Action<ProducerConfiguration>? configureProducer = null,
        Action<ConsumerConfiguration>? configureConsumer = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (string.IsNullOrWhiteSpace(serviceUrl))
            throw new ArgumentException("Service URL cannot be null or empty", nameof(serviceUrl));

        // Configure client
        services.Configure<WitiQPulsarClientConfiguration>(options =>
        {
            options.ServiceUrl = serviceUrl;
            configureClient?.Invoke(options);
        });

        // Configure default producer options
        if (configureProducer != null)
        {
            services.Configure<ProducerConfiguration>("Default", configureProducer);
        }

        // Configure default consumer options
        if (configureConsumer != null)
        {
            services.Configure<ConsumerConfiguration>("Default", configureConsumer);
        }

        // Register core services
        services.TryAddSingleton<IWitiQPulsarClient, WitiQPulsarClient>();
        services.TryAddSingleton<IWitiQPulsarFactory, WitiQPulsarFactory>();

        return services;
    }
}