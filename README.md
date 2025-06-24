# WitiQ.MessageBroker.Pulsar

A modern, robust .NET 8 library for integrating [Apache Pulsar](https://pulsar.apache.org/) into your .NET applications. Includes support for dependency injection, background consumers, health checks, and flexible configuration.

---

## Features

- **.NET 8** and async/await support
- Producer and consumer abstractions for Apache Pulsar
- Background consumer services with dependency injection
- Health checks for Pulsar connectivity
- Flexible configuration via `appsettings.json` or code
- TLS and token authentication support
- Logging via Microsoft.Extensions.Logging
- Automatic resource management and graceful shutdown

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Apache Pulsar server (local or remote)
- [DotPulsar](https://github.com/apache/pulsar-dotpulsar) (handled via NuGet)

### Installation

Add the NuGet packages to your project:

<PackageReference Include="WitiQ.MessageBroker.Pulsar.Core" Version="1.0.0" /> <PackageReference Include="WitiQ.MessageBroker.Pulsar.Extensions.Hosting" Version="1.0.0" />


---

## Configuration

Add your Pulsar settings to `appsettings.json`:


{ "WitiQPulsar": { "ServiceUrl": "pulsar://localhost:6650", "OperationTimeout": "00:00:30", "ConnectionTimeout": "00:00:10", "UseTls": false, "Authentication": { "Token": "" } } }


---

## Usage

### Registering Services

public void ConfigureServices(IServiceCollection services) { services.AddWitiQPulsar(Configuration.GetSection("WitiQPulsar")); services.AddWitiQPulsarHealthChecks(); }


### Sending Messages

public class MessageSender { private readonly IWitiQPulsarFactory _factory;
public MessageSender(IWitiQPulsarFactory factory)
{
    _factory = factory;
}

public async Task SendAsync(string message)
{
    using var producer = await _factory.CreateProducerAsync<string>("my-topic");
    await producer.SendAsync(message);
}
}


### Consuming Messages in Background

public class MyMessageHandler : IWitiQPulsarMessageHandler<string> { public async Task HandleAsync(WitiQPulsarMessage<string> message, CancellationToken cancellationToken) { // Process the message await Task.CompletedTask; } }
// Register the background consumer service services.AddWitiQPulsarConsumerBackgroundService<string, MyMessageHandler>( "my-topic", "my-subscription");


---

## Health Checks

Integrate with ASP.NET Core health checks:

app.UseHealthChecks("/health");


---

## Logging

Configure logging as usual with Microsoft.Extensions.Logging:


services.AddLogging(builder => { builder.AddConsole(); builder.SetMinimumLevel(LogLevel.Debug); });



---

## Testing

Run all unit tests with:


dotnet test


---

## Security

- **TLS**: Set `UseTls` to `true` and provide certificate paths in configuration.
- **Authentication**: Provide a valid Pulsar token in the `Authentication` section.

---

## Contributing

Contributions are welcome! Please fork the repository, create a feature branch, and submit a pull request.

---

## License

This project is licensed under the MIT License.

---

## Credits

- [DotPulsar](https://github.com/apache/pulsar-dotpulsar)
- [Apache Pulsar](https://pulsar.apache.org/)