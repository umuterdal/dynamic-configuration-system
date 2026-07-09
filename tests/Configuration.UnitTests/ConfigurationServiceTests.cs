using Configuration.Application.Services;
using Configuration.Domain.DTOs;
using Configuration.Domain.Entities;
using Configuration.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Configuration.UnitTests;

public class ConfigurationServiceTests : IDisposable
{
    private readonly Mock<IConfigurationRepository> _repositoryMock;
    private readonly Mock<ILogger<ConfigurationService>> _loggerMock;
    private readonly ConfigurationService _service;
    private readonly string _applicationName = "TEST-APP";

    public ConfigurationServiceTests()
    {
        _repositoryMock = new Mock<IConfigurationRepository>();
        _loggerMock = new Mock<ILogger<ConfigurationService>>();
        _service = new ConfigurationService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllConfigurationsAsync_ReturnsAllRecords()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "Key1", Type = "string", Value = "Value1", IsActive = 1, ApplicationName = _applicationName },
            new() { Id = "2", Name = "Key2", Type = "int", Value = "100", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _service.GetAllConfigurationsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetConfigurationsByApplicationAsync_ReturnsFilteredRecords()
    {
        // Arrange
        var records = new List<ConfigurationRecord>
        {
            new() { Id = "1", Name = "Key1", Type = "string", Value = "Value1", IsActive = 1, ApplicationName = _applicationName }
        };

        _repositoryMock.Setup(r => r.GetAllByApplicationNameAsync(_applicationName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var result = await _service.GetConfigurationsByApplicationAsync(_applicationName);

        // Assert
        result.Should().HaveCount(1);
        result[0].ApplicationName.Should().Be(_applicationName);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsRecord()
    {
        // Arrange
        var record = new ConfigurationRecord
        {
            Id = "1",
            Name = "Key1",
            Type = "string",
            Value = "Value1",
            IsActive = 1,
            ApplicationName = _applicationName
        };

        _repositoryMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _service.GetByIdAsync("1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("1");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync("999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConfigurationRecord?)null);

        // Act
        var result = await _service.GetByIdAsync("999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesRecord()
    {
        // Arrange
        var request = new CreateConfigurationRequest
        {
            Name = "NewKey",
            Type = "string",
            Value = "NewValue",
            IsActive = 1,
            ApplicationName = _applicationName
        };

        var createdRecord = new ConfigurationRecord
        {
            Id = "1",
            Name = request.Name,
            Type = request.Type,
            Value = request.Value,
            IsActive = request.IsActive,
            ApplicationName = request.ApplicationName
        };

        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<ConfigurationRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRecord);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("NewKey");
    }

    [Fact]
    public async Task CreateAsync_InvalidType_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateConfigurationRequest
        {
            Name = "NewKey",
            Type = "invalid",
            Value = "NewValue",
            IsActive = 1,
            ApplicationName = _applicationName
        };

        // Act & Assert
        var act = () => _service.CreateAsync(request);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesRecord()
    {
        // Arrange
        var request = new UpdateConfigurationRequest
        {
            Id = "1",
            Name = "UpdatedKey",
            Type = "string",
            Value = "UpdatedValue",
            IsActive = 1,
            ApplicationName = _applicationName
        };

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ConfigurationRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAsync(request);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesRecord()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync("1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.CreateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetConfigurationsByApplicationAsync_EmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => _service.GetConfigurationsByApplicationAsync("");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    public void Dispose()
    {
        _repositoryMock.Reset();
        _loggerMock.Reset();
    }
}
