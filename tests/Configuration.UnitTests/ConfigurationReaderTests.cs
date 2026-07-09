using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using Configuration.Library;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Configuration.UnitTests;

public class ConfigurationReaderTests : IDisposable
{
    private readonly Mock<IConfigurationRepository> _repositoryMock;
    private readonly Mock<ILogger<ConfigurationReader>> _loggerMock;
    private readonly string _applicationName = "TEST-APP";

    public ConfigurationReaderTests()
    {
        _repositoryMock = new Mock<IConfigurationRepository>();
        _loggerMock = new Mock<ILogger<ConfigurationReader>>();
    }

    [Fact]
    public void GetValue_WithExistingKey_ReturnsStringValue()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "SiteName", Type = "string", Value = "test.com", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        var result = reader.GetValue<string>("SiteName");

        // Assert
        result.Should().Be("test.com");
    }

    [Fact]
    public void GetValue_WithIntType_ReturnsIntValue()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "MaxItems", Type = "int", Value = "100", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        var result = reader.GetValue<int>("MaxItems");

        // Assert
        result.Should().Be(100);
    }

    [Fact]
    public void GetValue_WithDoubleType_ReturnsDoubleValue()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "TaxRate", Type = "double", Value = "0.18", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        var result = reader.GetValue<double>("TaxRate");

        // Assert
        result.Should().Be(0.18);
    }

    [Fact]
    public void GetValue_WithBoolType_ReturnsBoolValue()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "IsEnabled", Type = "bool", Value = "true", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        var result = reader.GetValue<bool>("IsEnabled");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetValue_WithMissingKey_ThrowsKeyNotFoundException()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "SiteName", Type = "string", Value = "test.com", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act & Assert
        var act = () => reader.GetValue<string>("NonExistentKey");
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void GetValue_WithInactiveKey_ThrowsKeyNotFoundException()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "SiteName", Type = "string", Value = "test.com", IsActive = 0, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act & Assert
        var act = () => reader.GetValue<string>("SiteName");
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void GetValue_WithInvalidTypeConversion_ThrowsInvalidCastException()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "Count", Type = "int", Value = "not_a_number", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act & Assert
        var act = () => reader.GetValue<int>("Count");
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void TryGetValue_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "SiteName", Type = "string", Value = "test.com", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        var result = reader.TryGetValue<string>("SiteName", out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be("test.com");
    }

    [Fact]
    public void TryGetValue_WithMissingKey_ReturnsFalse()
    {
        // Arrange
        var records = new List<ConfigurationRecord>();
        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        var result = reader.TryGetValue<string>("NonExistent", out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void GetAllValues_ReturnsOnlyActiveRecords()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "Active1", Type = "string", Value = "val1", IsActive = 1, ApplicationName = _applicationName },
            new() { Id = "2", Name = "Inactive1", Type = "string", Value = "val2", IsActive = 0, ApplicationName = _applicationName },
            new() { Id = "3", Name = "Active2", Type = "string", Value = "val3", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        var result = reader.GetAllValues();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey("Active1");
        result.Should().ContainKey("Active2");
        result.Should().NotContainKey("Inactive1");
    }

    [Fact]
    public void GetValue_IsCaseInsensitive()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "SiteName", Type = "string", Value = "test.com", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        var result = reader.GetValue<string>("sitename");

        // Assert
        result.Should().Be("test.com");
    }

    [Fact]
    public async Task RefreshAsync_UpdatesCacheWithNewValues()
    {
        // Arrange
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

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);

        // Act
        await reader.RefreshAsync();

        // Assert
        var result = reader.GetValue<string>("Key1");
        result.Should().Be("UpdatedValue");
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ConfigurationReader(_applicationName, null!, 60000, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithInvalidRefreshInterval_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        var act = () => new ConfigurationReader(_applicationName, _repositoryMock.Object, -1, _loggerMock.Object);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetValue_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var records = new List<ConfigurationRecord>();
        _repositoryMock.Setup(r => r.GetByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        var reader = new ConfigurationReader(_applicationName, _repositoryMock.Object, 60000, _loggerMock.Object);
        reader.Dispose();

        // Act & Assert
        var act = () => reader.GetValue<string>("Key");
        act.Should().Throw<ObjectDisposedException>();
    }

    public void Dispose()
    {
        _repositoryMock.Reset();
        _loggerMock.Reset();
    }
}
