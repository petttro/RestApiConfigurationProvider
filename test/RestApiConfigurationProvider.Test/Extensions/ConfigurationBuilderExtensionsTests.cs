using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestApiConfigurationProvider.ConfigurationProviders;
using RestApiConfigurationProvider.Extensions;
using RestApiConfigurationProvider.HttpClients;
using Xunit;

namespace RestApiConfigurationProvider.Test.Extensions;

public class ConfigurationBuilderExtensionsTests
{
    [Fact]
    public void AddRestApiConfigurationSource_AddsSourceCorrectly()
    {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        var distributedCacheMock = new Mock<IDistributedCache>();
        var restApiHttpClientMock = new Mock<IRestApiHttpClient>();
        var restApiConfigurationProviderSettings = new RestApiConfigurationProviderSettings
        {
            ConfigurationKeys = new List<string> { "key1", "key2", "key3" },
            CacheDurationMinutes = 10,
            CacheKeyPrefix = "distributed_cache_prefix"
        };

        // Act
        var result = configurationBuilder.AddRestApiConfigurationSource(
            new NullLoggerFactory(),
            distributedCacheMock.Object,
            restApiHttpClientMock.Object,
            restApiConfigurationProviderSettings
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(configurationBuilder, result);
        Assert.Contains(configurationBuilder.Sources, source => source is RestApiConfigurationSource);
    }
}
