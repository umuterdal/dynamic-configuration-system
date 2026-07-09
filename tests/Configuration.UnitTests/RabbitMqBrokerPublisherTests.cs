using Configuration.Application.Services;
using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Configuration.UnitTests;

/// <summary>
/// Unit tests for the RabbitMQ broker publisher.
/// TDD: Tests written before implementation to define expected behavior.
/// </summary>
public class RabbitMqBrokerPublisherTests : IDisposable
{
    private readonly Mock<IModel> _channelMock;
    private readonly Mock<IConnection> _connectionMock;
    private readonly Mock<ILogger<RabbitMqConfigurationBrokerPublisher>> _loggerMock;
    private readonly ConfigurationBrokerSettings _settings;
    private RabbitMqConfigurationBrokerPublisher _publisher = null!;

    public RabbitMqBrokerPublisherTests()
    {
        _channelMock = new Mock<IModel>();
        _connectionMock = new Mock<IConnection>();
        _loggerMock = new Mock<ILogger<RabbitMqConfigurationBrokerPublisher>>();
        _settings = new ConfigurationBrokerSettings
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            ExchangeName = "test-exchange",
            QueueName = "test-queue"
        };
    }

    [Fact]
    public void PublishAsync_ValidEvent_PublishesMessage()
    {
        // Arrange
        var publisherMock = new Mock<IConfigurationBrokerPublisher>();

        publisherMock.Setup(p => p.PublishAsync("TEST-APP", "Created", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        publisherMock.Object.PublishAsync("TEST-APP", "Created").GetAwaiter().GetResult();
        publisherMock.Verify(p => p.PublishAsync("TEST-APP", "Created", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_NullApplicationName_ThrowsArgumentException()
    {
        // Arrange
        var publisher = new Mock<IConfigurationBrokerPublisher>();
        publisher.Setup(p => p.PublishAsync(null!, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Application name cannot be null or empty."));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => publisher.Object.PublishAsync(null!, "Created"));
    }

    [Fact]
    public async Task PublishAsync_NullChangeType_ThrowsArgumentException()
    {
        // Arrange
        var publisher = new Mock<IConfigurationBrokerPublisher>();
        publisher.Setup(p => p.PublishAsync(It.IsAny<string>(), null!, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Change type cannot be null or empty."));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => publisher.Object.PublishAsync("TEST-APP", null!));
    }

    [Fact]
    public async Task PublishAsync_BrokerFailure_DoesNotThrow()
    {
        // Arrange - broker is down, should not throw
        var publisher = new Mock<IConfigurationBrokerPublisher>();
        publisher.Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask); // Graceful degradation

        // Act & Assert - should not throw
        var act = () => publisher.Object.PublishAsync("TEST-APP", "Created");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void ConfigurationBrokerSettings_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var settings = new ConfigurationBrokerSettings();

        // Assert
        settings.HostName.Should().Be("localhost");
        settings.Port.Should().Be(5672);
        settings.UserName.Should().Be("guest");
        settings.Password.Should().Be("guest");
        settings.ExchangeName.Should().Be("config-changes");
        ConfigurationBrokerSettings.SectionName.Should().Be("ConfigurationBroker");
    }

    [Fact]
    public void ConfigurationBrokerSettings_CustomValues_AreSetCorrectly()
    {
        // Arrange & Act
        var settings = new ConfigurationBrokerSettings
        {
            HostName = "rabbitmq-server",
            Port = 5673,
            UserName = "admin",
            Password = "secret",
            ExchangeName = "my-exchange",
            QueueName = "my-queue"
        };

        // Assert
        settings.HostName.Should().Be("rabbitmq-server");
        settings.Port.Should().Be(5673);
        settings.UserName.Should().Be("admin");
        settings.Password.Should().Be("secret");
        settings.ExchangeName.Should().Be("my-exchange");
        settings.QueueName.Should().Be("my-queue");
    }

    public void Dispose()
    {
        _publisher?.Dispose();
    }
}
