﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using RestApiConfigurationProvider.ConfigurationProviders;
using RestApiConfigurationProvider.HttpClients;
using Xunit;

namespace RestApiConfigurationProvider.Test.ConfigurationProvider;

public class RestApiConfigurationSourceTests : MockStrictBehaviorTest
{
    private readonly RestApiConfigurationSource _configurationSource;

    public RestApiConfigurationSourceTests()
    {
        var distributedCacheMock = _mockRepository.Create<IDistributedCache>();
        var restApiHttpClient = _mockRepository.Create<IRestApiHttpClient>();

        _configurationSource = new RestApiConfigurationSource(
            new NullLoggerFactory(),
            distributedCacheMock.Object,
            restApiHttpClient.Object,
            new RestApiConfigurationProviderSettings());
    }

    [Fact]
    public void Build_CreatesOnlyOneInstance()
    {
        var configurationBuilderMock = _mockRepository.Create<IConfigurationBuilder>();
        var configurationProvider1 = _configurationSource.Build(configurationBuilderMock.Object);
        var configurationProvider2 = _configurationSource.Build(configurationBuilderMock.Object);

        Assert.True(configurationProvider1 is ConfigurationProviders.RestApiConfigurationProvider);
        Assert.Equal(configurationProvider1, configurationProvider2);
    }
}
