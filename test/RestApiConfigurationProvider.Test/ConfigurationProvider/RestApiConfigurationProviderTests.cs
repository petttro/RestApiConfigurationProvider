using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestApiConfigurationProvider.ConfigurationProviders;
using RestApiConfigurationProvider.Extensions;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;
using Xunit;

namespace RestApiConfigurationProvider.Test.ConfigurationProvider;

public class RestApiConfigurationProviderTests : MockStrictBehaviorTest
{
    private readonly Mock<IConfigurationRestApiClient> _configurationServiceMock;
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly ConfigurationProviders.RestApiConfigurationProvider _provider;
    private readonly RestApiConfigurationProviderSettings _restApiConfigurationProviderSettings;

    public RestApiConfigurationProviderTests()
    {
        _configurationServiceMock = new Mock<IConfigurationRestApiClient>(MockBehavior.Strict);
        _distributedCacheMock = new Mock<IDistributedCache>(MockBehavior.Strict);

        _restApiConfigurationProviderSettings = new RestApiConfigurationProviderSettings
        {
            CacheDurationMinutes = 10,
            CacheKeyPrefix = "cache_key_prefix",
            ConfigurationKeys = new List<string> { "ConfigurationSet1", "ConfigurationSet2" }
        };

        _provider = new ConfigurationProviders.RestApiConfigurationProvider(
            _configurationServiceMock.Object,
            _distributedCacheMock.Object,
            _restApiConfigurationProviderSettings,
            NullLogger.Instance);
    }

    [Fact]
    public async Task LoadAsync_LoadsFromCache_IfDataExists()
    {
        var configurationKey1 = _restApiConfigurationProviderSettings.ConfigurationKeys[0];
        var cacheKey1 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey1}";

        var configurationKey2 = _restApiConfigurationProviderSettings.ConfigurationKeys[1];
        var cacheKey2 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey2}";

        var mockData = new Dictionary<string, string> { { "SubKey1", "Value1" } };

        var serializedMockData = JsonSerializer.SerializeToUtf8Bytes(mockData);
        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey1, default))
            .ReturnsAsync(serializedMockData);

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey2, default))
            .ReturnsAsync(serializedMockData);

        await _provider.LoadAsync();

        Assert.True(_provider.TryGet("SubKey1", out var result) && result == "Value1");
    }

    [Fact]
    public async Task LoadAsync_LoadsFromRestApi_IfCacheIsEmpty()
    {
        var configurationKey1 = _restApiConfigurationProviderSettings.ConfigurationKeys[0];
        var cacheKey1 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey1}";

        var configurationKey2 = _restApiConfigurationProviderSettings.ConfigurationKeys[1];
        var cacheKey2 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey2}";

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        var mockData1 = new ConfigurationResponse()
        {
            ConfigurationValue = new
            {
                SubKey1 = "Value1",
                SubKey2 = "Value2",
                SubKey3 = "Value3",
                SubKey4 = new[] { "a", "b", "c" }
            }.SerializeJsonSafe()
        };

        var mockData2 = new ConfigurationResponse()
        {
            ConfigurationValue = new
            {
                SubKey1 = "Value1",
                SubKey2 = "Value2",
                SubKey3 = "Value3"
            }.SerializeJsonSafe()
        };

        _configurationServiceMock
            .Setup(x => x.GetConfigurationAsync(configurationKey1, false))
            .ReturnsAsync(mockData1);

        _configurationServiceMock
            .Setup(x => x.GetConfigurationAsync(configurationKey2, false))
            .ReturnsAsync(mockData2);

        _distributedCacheMock
            .Setup(x => x.SetAsync(cacheKey1, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _distributedCacheMock
            .Setup(x => x.SetAsync(cacheKey2, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _provider.LoadAsync();

        Assert.True(_provider.TryGet("SubKey1", out var result) && result == "Value1");
    }

    [Fact]
    public async Task LoadAsync_LoadsFromRestApi_OneConfigThrowsException()
    {
        var configurationKey1 = _restApiConfigurationProviderSettings.ConfigurationKeys[0];
        var cacheKey1 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey1}";

        var configurationKey2 = _restApiConfigurationProviderSettings.ConfigurationKeys[1];
        var cacheKey2 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey2}";

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _configurationServiceMock
            .Setup(x => x.GetConfigurationAsync(configurationKey1, false))
            .Throws<Exception>();

        var mockData2 = new ConfigurationResponse()
        {
            ConfigurationValue = new
            {
                SubKey1 = "Value1",
                SubKey2 = "Value2",
                SubKey3 = "Value3"
            }.SerializeJsonSafe()
        };

        _configurationServiceMock
            .Setup(x => x.GetConfigurationAsync(configurationKey2, false))
            .ReturnsAsync(mockData2);

        _distributedCacheMock
            .Setup(x => x.SetAsync(cacheKey2, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _provider.LoadAsync();

        _distributedCacheMock.Verify(x => x.GetAsync(cacheKey1, It.IsAny<CancellationToken>()), Times.Once);
        _distributedCacheMock.Verify(x => x.GetAsync(cacheKey2, It.IsAny<CancellationToken>()), Times.Once);

        // Verify that the Set method was NOT called
        _distributedCacheMock.Verify(x =>
            x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadAsync_LoadsFromRestApi_OneConfigInvalidJson()
    {
        var configurationKey1 = _restApiConfigurationProviderSettings.ConfigurationKeys[0];
        var cacheKey1 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey1}";

        var configurationKey2 = _restApiConfigurationProviderSettings.ConfigurationKeys[1];
        var cacheKey2 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey2}";

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        var mockData1 = new ConfigurationResponse()
        {
            ConfigurationValue = "wrong json"
        };

        _configurationServiceMock
            .Setup(x => x.GetConfigurationAsync(configurationKey1, false))
            .ReturnsAsync(mockData1);

        var mockData2 = new ConfigurationResponse()
        {
            ConfigurationValue = new
            {
                SubKey1 = "Value1",
                SubKey2 = "Value2",
                SubKey3 = "Value3"
            }.SerializeJsonSafe()
        };

        _configurationServiceMock
            .Setup(x => x.GetConfigurationAsync(configurationKey2, false))
            .ReturnsAsync(mockData2);

        _distributedCacheMock
            .Setup(x => x.SetAsync(cacheKey2, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _provider.LoadAsync();

        _distributedCacheMock.Verify(x => x.GetAsync(cacheKey1, It.IsAny<CancellationToken>()), Times.Once);
        _distributedCacheMock.Verify(x => x.GetAsync(cacheKey2, It.IsAny<CancellationToken>()), Times.Once);

        // Verify that the Set method was NOT called
        _distributedCacheMock.Verify(x =>
            x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoadAsync_LoadsFromRestApi_ConfigIsNull()
    {
        var configurationKey1 = _restApiConfigurationProviderSettings.ConfigurationKeys[0];
        var cacheKey1 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey1}";

        var configurationKey2 = _restApiConfigurationProviderSettings.ConfigurationKeys[1];
        var cacheKey2 = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey2}";

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _distributedCacheMock
            .Setup(x => x.GetAsync(cacheKey2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        _configurationServiceMock
            .Setup(x => x.GetConfigurationAsync(configurationKey1, false))
            .ReturnsAsync((ConfigurationResponse)null!);

        await _provider.LoadAsync();

        _distributedCacheMock.Verify(x => x.GetAsync(cacheKey1, It.IsAny<CancellationToken>()), Times.Once);
        _distributedCacheMock.Verify(x => x.GetAsync(cacheKey2, It.IsAny<CancellationToken>()), Times.Once);

        // Verify that the Set method was NOT called
        _distributedCacheMock.Verify(x =>
            x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }



    [Fact]
    public async Task InvalidateCacheAsync_UpdatesSpecificConfiguration()
    {
        var configurationKey = "config_key";
        var cacheKey = $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey}";

        // Mock the underlying Set method instead of SetAsync
        _distributedCacheMock
            .Setup(x => x.RemoveAsync(cacheKey, new CancellationToken()))
            .Returns(Task.CompletedTask);

        await _provider.InvalidateCacheAsync(configurationKey);
    }
}
