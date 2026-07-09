using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Configuration.Library;
using Configuration.Library.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Configuration.UnitTests;

/// <summary>
/// Unit tests for the ConfigurationBrokerConsumer.
/// TDD: Tests written before implementation to define expected behavior.
/// </summary>
public class ConfigurationBrokerConsumerTests
{
    private readonly Mock<IConfigurationRepository> _repositoryMock;
    private readonly Mock<ILogger<ConfigurationReader>> _readerLoggerMock;
    private readonly Mock<ILogger<ConfigurationBrokerConsumer>> _consumerLoggerMock;
    private readonly ConfigurationBrokerSettings _brokerSettings;
    private readonly string _applicationName = "TEST-APP";

    public ConfigurationBrokerConsumerTests()
    {
        _repositoryMock = new Mock<IConfigurationRepository>();
        _readerLoggerMock = new Mock<ILogger<ConfigurationReader>>();
        _consumerLoggerMock = new Mock<ILogger<ConfigurationBrokerConsumer>>();
        _brokerSettings = new ConfigurationBrokerSettings
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            ExchangeName = "test-exchange",
            QueueName = "test-queue"
        };

        // Setup default empty records for ConfigurationReader initialization
        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConfigurationRecord>());
    }

    [Fact]
    public void Constructor_NullConfigurationReader_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ConfigurationBrokerConsumer(
            null!,
            Options.Create(_brokerSettings),
            _consumerLoggerMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("configurationReader");
    }

    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        var reader = new ConfigurationReader(
            _applicationName,
            _repositoryMock.Object,
            60000,
            _readerLoggerMock.Object);

        // Act & Assert
        var act = () => new ConfigurationBrokerConsumer(
            reader,
            null!,
            _consumerLoggerMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("settings");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var reader = new ConfigurationReader(
            _applicationName,
            _repositoryMock.Object,
            60000,
            _readerLoggerMock.Object);

        // Act & Assert
        var act = () => new ConfigurationBrokerConsumer(
            reader,
            Options.Create(_brokerSettings),
            null!);

        act.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesConsumer()
    {
        // Arrange
        var reader = new ConfigurationReader(
            _applicationName,
            _repositoryMock.Object,
            60000,
            _readerLoggerMock.Object);

        // Act
        var consumer = new ConfigurationBrokerConsumer(
            reader,
            Options.Create(_brokerSettings),
            _consumerLoggerMock.Object);

        // Assert
        consumer.Should().NotBeNull();
    }

    [Fact]
    public async Task ConfigurationReader_RefreshAsync_WorksCorrectly()
    {
        // Arrange - This tests the core mechanism that the broker consumer triggers
        var initialRecords = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "Key1", Type = "string", Value = "Value1", IsActive = 1, ApplicationName = _applicationName }
        };

        var updatedRecords = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "Key1", Type = "string", Value = "UpdatedValue", IsActive = 1, ApplicationName = _applicationName }
        };

        var callCount = 0;
        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++callCount == 1 ? initialRecords : updatedRecords);

        var reader = new ConfigurationReader(
            _applicationName,
            _repositoryMock.Object,
            60000,
            _readerLoggerMock.Object);

        // Act - This is what the broker consumer does when it receives a message
        await reader.RefreshAsync();

        // Assert
        var result = reader.GetValue<string>("Key1");
        result.Should().Be("UpdatedValue");
    }

    [Fact]
    public void ConfigurationBrokerSettings_SectionName_IsCorrect()
    {
        // Assert
        ConfigurationBrokerSettings.SectionName.Should().Be("ConfigurationBroker");
    }

    [Fact]
    public void ConfigurationReader_Constructor_WithThreeParameters_AcceptsConnectionString()
    {
        // This test verifies the public 3-parameter constructor signature
        // We can't test with a real MongoDB connection, but we verify the signature exists
        var constructor = typeof(ConfigurationReader).GetConstructor(
            new[] { typeof(string), typeof(string), typeof(int) });

        // Assert
        constructor.Should().NotBeNull("ConfigurationReader should have a 3-parameter constructor");
        constructor!.GetParameters().Length.Should().Be(3);
        constructor.GetParameters()[0].Name.Should().Be("applicationName");
        constructor.GetParameters()[1].Name.Should().Be("connectionString");
        constructor.GetParameters()[2].Name.Should().Be("refreshTimerIntervalInMs");
    }
}
