using System.Net.Mime;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using RestApiConfigurationProvider.Exceptions;
using RestApiConfigurationProvider.Extensions;
using RestApiConfigurationProvider.HttpClients;
using RestApiConfigurationProvider.HttpClients.DTO.Requests;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;
using Xunit;

namespace RestApiConfigurationProvider.Test;

public class ConfigurationRestApiClientTests : MockStrictBehaviorTest
{
    private readonly ConfigurationRestApiClient _client;
    private readonly Mock<IRestApiHttpClient> _httpClientMock;

    public ConfigurationRestApiClientTests()
    {
        _httpClientMock = _mockRepository.Create<IRestApiHttpClient>();
        _client = new ConfigurationRestApiClient(_httpClientMock.Object, NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task UpsertConfigurationAsync_Success()
    {
        // Arrange
        var configurationKey = "testKey";
        var configurationValue = new { Value = "test" };
        var cacheDuration = 44;
        var comments = "comments to config";
        var configurationChangeResponse = new ConfigurationChangeResponse
        {
            ConfigurationVersion = 23,
            LastUpdateDateTime = DateTime.UtcNow
        };

        _httpClientMock
            .Setup(x => x.UpsertConfigurationAsync(configurationKey, It.Is<ConfigurationsChangeRequest>(r =>
                r.ConfigurationValue == configurationValue.SerializeJsonSafe(null) &&
                r.ContentType == MediaTypeNames.Application.Json &&
                r.CacheDurationSeconds ==  cacheDuration &&
                r.Comments == comments &&
                r.LastChangedBy == "RestApiConfigurationProvider")))
            .ReturnsAsync(configurationChangeResponse);

        // Act
        await _client.UpsertConfigurationAsync(configurationKey, configurationValue, cacheDuration, comments);
    }

    [Fact]
    public async Task UpsertConfigurationAsync_SerializationFailure_ThrowsException()
    {
        // Arrange
        var invalidObject = new InvalidMockObject(); // Assume this object causes JSON serialization to fail

        // Act & Assert
        await Assert.ThrowsAsync<JsonSerializationException>(() => _client.UpsertConfigurationAsync("testKey", invalidObject));
    }

    [Fact]
    public async Task UpsertConfigurationAsync_HttpClientFailure_ThrowsException()
    {
        var key = "testKey";
        // Arrange
        _httpClientMock
            .Setup(x => x.UpsertConfigurationAsync(key, It.IsAny<ConfigurationsChangeRequest>()))
            .ThrowsAsync(new Exception("HTTP Client Failure"));

        // Act & Assert
        await Assert.ThrowsAsync<ConfigurationException>(() => _client.UpsertConfigurationAsync(key, new { Value = "test" }));
    }

    [Fact]
    public async Task GetConfigurationAsync_Success()
    {
        // Arrange
        var expectedResponse = new ConfigurationResponse();
        _httpClientMock
            .Setup(x => x.GetCurrentConfigurationAsync("testKey"))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.GetConfigurationAsync("testKey");

        // Assert
        Assert.Same(expectedResponse, response);
    }

    [Fact]
    public async Task GetConfigurationAsync_HttpClientFailure_ThrowsException()
    {
        // Arrange
        _httpClientMock
            .Setup(x => x.GetCurrentConfigurationAsync("testKey"))
            .ThrowsAsync(new Exception("HTTP Client Failure"));

        // Act & Assert
        await Assert.ThrowsAsync<ConfigurationException>(() => _client.GetConfigurationAsync("testKey"));
    }

    [Fact]
    public async Task GetConfigurationAsync_NullConfiguration_ThrowsException_When_ThrowIfNull_Is_True()
    {
        // Arrange
        _httpClientMock
            .Setup(x => x.GetCurrentConfigurationAsync("testKey"))
            .ReturnsAsync((ConfigurationResponse)null!);

        // Act & Assert
        await Assert.ThrowsAsync<ConfigurationException>(() => _client.GetConfigurationAsync("testKey", throwIfNull: true));
    }

    [Fact]
    public async Task GetConfigurationAsync_NullConfiguration_ReturnsNull_When_ThrowIfNull_Is_False()
    {
        // Arrange
        _httpClientMock
            .Setup(x => x.GetCurrentConfigurationAsync("testKey"))
            .ReturnsAsync((ConfigurationResponse)null!);

        // Act
        var result = await _client.GetConfigurationAsync("testKey", throwIfNull: false);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetConfigurationValueAsync_Success()
    {
        // Arrange
        var expectedResponse = new ConfigurationResponse
        {
            Application = "application",
            Comments = "comments",
            ConfigurationValue = JsonConvert.SerializeObject("testValue"),
            ConfigurationVersion = 34,
            ContentType = MediaTypeNames.Application.Json,
            ConfigurationSetId = "testKey",
            LastChangedBy = "last changed by",
            LastUpdateDateTime = DateTime.UtcNow
        };
        _httpClientMock
            .Setup(x => x.GetCurrentConfigurationAsync("testKey"))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await _client.GetConfigurationValueAsync<string>("testKey");

        // Assert
        Assert.Equal("testValue", response);
    }

    [Fact]
    public async Task GetConfigurationValueAsync_DeserializationFailure_ThrowsException()
    {
        // Arrange
        var expectedResponse = new ConfigurationResponse { ConfigurationValue = "invalid_json" };
        _httpClientMock
            .Setup(x => x.GetCurrentConfigurationAsync("testKey"))
            .ReturnsAsync(expectedResponse);

        // Act & Assert
        await Assert.ThrowsAsync<ConfigurationException>(() => _client.GetConfigurationValueAsync<string>("testKey"));
    }

    public class InvalidMockObject
    {
        public MemoryStream Stream { get; set; } = new MemoryStream();
    }
}
