using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using WitiQ.MessageBroker.Pulsar.Core.Abstractions;
using WitiQ.MessageBroker.Pulsar.Extensions.Hosting;
using Xunit;

namespace WitiQ.MessageBroker.Pulsar.Tests.Services
{
    public class WitiQPulsarHealthCheckTests
    {
        private readonly Mock<IWitiQPulsarClient> _clientMock;
        private readonly Mock<ILogger<WitiQPulsarHealthCheck>> _loggerMock;
        private readonly WitiQPulsarHealthCheck _healthCheck;
        private readonly HealthCheckContext _context;

        public WitiQPulsarHealthCheckTests()
        {
            _clientMock = new Mock<IWitiQPulsarClient>();
            _loggerMock = new Mock<ILogger<WitiQPulsarHealthCheck>>();
            _healthCheck = new WitiQPulsarHealthCheck(_clientMock.Object, _loggerMock.Object);
            _context = new HealthCheckContext();
        }

        [Fact]
        public void Constructor_NullClient_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WitiQPulsarHealthCheck(null!, _loggerMock.Object));
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WitiQPulsarHealthCheck(_clientMock.Object, null!));
        }

        [Fact]
        public async Task CheckHealthAsync_ClientNotConnected_ReturnsUnhealthy()
        {
            // Arrange
            _clientMock.Setup(x => x.IsConnected).Returns(false);

            // Act
            var result = await _healthCheck.CheckHealthAsync(_context);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.Equal("Pulsar client is not connected", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_ClientConnectedAndProducerCreated_ReturnsHealthy()
        {
            // Arrange
            _clientMock.Setup(x => x.IsConnected).Returns(true);
            var producer = Mock.Of<IWitiQPulsarProducer<string>>();
            _clientMock.Setup(x => x.CreateProducerAsync<string>("health-check-topic", null, default))
                .ReturnsAsync(producer);

            // Act
            var result = await _healthCheck.CheckHealthAsync(_context);

            // Assert
            Assert.Equal(HealthStatus.Healthy, result.Status);
            Assert.Equal("Pulsar client is connected and operational", result.Description);
        }

        [Fact]
        public async Task CheckHealthAsync_ExceptionThrown_ReturnsUnhealthy()
        {
            // Arrange
            _clientMock.Setup(x => x.IsConnected).Returns(true);
            _clientMock.Setup(x => x.CreateProducerAsync<string>(It.IsAny<string>(), null, default))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _healthCheck.CheckHealthAsync(_context);

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, result.Status);
            Assert.StartsWith("Pulsar client health check failed", result.Description);
        }
    }
}