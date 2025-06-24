using Microsoft.Extensions.Logging;
using Moq;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Core.Configuration;
using WitiQ.MessageBroker.Pulsar.Core.Services;
using Xunit;

namespace WitiQ.MessageBroker.Pulsar.Tests.Services
{
    public class WitiQPulsarFactoryTests
    {
        private readonly Mock<IWitiQPulsarClient> _clientMock;
        private readonly Mock<ILogger<WitiQPulsarFactory>> _loggerMock;
        private readonly WitiQPulsarFactory _factory;

        public WitiQPulsarFactoryTests()
        {
            _clientMock = new Mock<IWitiQPulsarClient>();
            _loggerMock = new Mock<ILogger<WitiQPulsarFactory>>();
            _factory = new WitiQPulsarFactory(_clientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_NullClient_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new WitiQPulsarFactory(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new WitiQPulsarFactory(_clientMock.Object, null!));
        }

        [Fact]
        public async Task CreateProducerAsync_ValidParameters_CallsClientAndReturnsProducer()
        {
            // Arrange
            var topic = "test-topic";
            var config = new ProducerConfiguration();
            var producer = Mock.Of<IWitiQPulsarProducer<string>>();
            
            _clientMock.Setup(x => x.CreateProducerAsync<string>(topic, config, default))
                .ReturnsAsync(producer);

            // Act
            var result = await _factory.CreateProducerAsync<string>(topic, config);

            // Assert
            Assert.Same(producer, result);
            _clientMock.Verify(x => x.CreateProducerAsync<string>(topic, config, default), Times.Once);
        }

        [Fact]
        public async Task CreateConsumerAsync_ValidParameters_CallsClientAndReturnsConsumer()
        {
            // Arrange
            var topic = "test-topic";
            var subscription = "test-subscription";
            var config = new ConsumerConfiguration();
            var consumer = Mock.Of<IWitiQPulsarConsumer<string>>();

            _clientMock.Setup(x => x.CreateConsumerAsync<string>(topic, subscription, config, default))
                .ReturnsAsync(consumer);

            // Act
            var result = await _factory.CreateConsumerAsync<string>(topic, subscription, config);

            // Assert
            Assert.Same(consumer, result);
            _clientMock.Verify(x => x.CreateConsumerAsync<string>(topic, subscription, config, default), Times.Once);
        }
    }
}