using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;
using WitiQ.MessageBroker.Pulsar.Core.Models;
using WitiQ.MessageBroker.Pulsar.Extensions.Hosting.Services;
using Xunit;

namespace WitiQ.MessageBroker.Pulsar.Tests.Services
{
    public class WitiQPulsarConsumerBackgroundServiceTests
    {
        private readonly Mock<IWitiQPulsarFactory> _factoryMock;
        private readonly Mock<IWitiQPulsarConsumer<string>> _consumerMock;
        private readonly Mock<IWitiQPulsarMessageHandler<string>> _handlerMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<ILogger<WitiQPulsarConsumerBackgroundService<string, IWitiQPulsarMessageHandler<string>>>> _loggerMock;
        private readonly string _topic = "test-topic";
        private readonly string _subscription = "test-subscription";
        private readonly ConsumerConfiguration _config;
        private readonly WitiQPulsarConsumerBackgroundService<string, IWitiQPulsarMessageHandler<string>> _service;

        public WitiQPulsarConsumerBackgroundServiceTests()
        {
            _factoryMock = new Mock<IWitiQPulsarFactory>();
            _consumerMock = new Mock<IWitiQPulsarConsumer<string>>();
            _handlerMock = new Mock<IWitiQPulsarMessageHandler<string>>();
            _scopeMock = new Mock<IServiceScope>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _loggerMock = new Mock<ILogger<WitiQPulsarConsumerBackgroundService<string, IWitiQPulsarMessageHandler<string>>>>();
            _config = new ConsumerConfiguration();

            _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_scopeFactoryMock.Object);
            _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);

            _serviceProviderMock.Setup(x => x.GetService(typeof(IWitiQPulsarMessageHandler<string>)))
                .Returns(_handlerMock.Object);

            _factoryMock.Setup(x => x.CreateConsumerAsync<string>(_topic, _subscription, _config, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_consumerMock.Object);

            _service = new WitiQPulsarConsumerBackgroundService<string, IWitiQPulsarMessageHandler<string>>(
                _factoryMock.Object,
                _topic,
                _subscription,
                _serviceProviderMock.Object,
                _config,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ProcessesMessages_UntilCancelled()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var message = new WitiQPulsarMessage<string>
            {
                MessageId = "test-message-id",
                Data = "test-message"
            };

            _consumerMock.SetupSequence(x => x.ReceiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(message)
                .ThrowsAsync(new OperationCanceledException());

            // Act
            var executeTask = _service.StartAsync(cts.Token);
            await Task.Delay(100); // Da tiempo a que procese el mensaje
            cts.Cancel();
            await executeTask;

            // Assert
            _factoryMock.Verify(x => x.CreateConsumerAsync<string>(_topic, _subscription, _config, It.IsAny<CancellationToken>()), Times.Once);
            _handlerMock.Verify(x => x.HandleAsync(It.IsAny<WitiQPulsarMessage<string>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _consumerMock.Verify(x => x.AcknowledgeAsync(It.IsAny<WitiQPulsarMessage<string>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ProcessMessage_HandlerThrowsException_LogsErrorAndContinues()
        {
            // Arrange
            var message = new WitiQPulsarMessage<string>
            {
                MessageId = "test-message-id",
                Data = "test-message"
            };

            _consumerMock.SetupSequence(x => x.ReceiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(message)
                .ThrowsAsync(new OperationCanceledException()); // Para salir del bucle

            _handlerMock.Setup(x => x.HandleAsync(It.IsAny<WitiQPulsarMessage<string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var cts = new CancellationTokenSource();
            await _service.StartAsync(cts.Token);
            await Task.Delay(100); // Dar tiempo para que se procese el mensaje
            cts.Cancel();
            await _service.StopAsync(cts.Token);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(3));
        }

        [Fact]
        public async Task StopAsync_DisposesConsumer()
        {
            // Act
            await _service.StartAsync(CancellationToken.None);
            await _service.StopAsync(CancellationToken.None);

            // Assert
            _consumerMock.Verify(x => x.Dispose(), Times.Once);
        }
    }
}