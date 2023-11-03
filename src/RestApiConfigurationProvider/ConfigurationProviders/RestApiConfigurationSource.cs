using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestApiConfigurationProvider.HttpClients;

namespace RestApiConfigurationProvider.ConfigurationProviders;

internal class RestApiConfigurationSource : IConfigurationSource
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDistributedCache _distributedCache;
    private readonly IRestApiHttpClient _restApiHttpClient;
    private readonly RestApiConfigurationProviderSettings _restApiConfigurationProviderSettings;

    private readonly ILogger _logger;
    private IConfigurationProvider _configurationProvider;

    public RestApiConfigurationSource(
        ILoggerFactory loggerFactory,
        IDistributedCache distributedCache,
        IRestApiHttpClient restApiHttpClient,
        RestApiConfigurationProviderSettings configurationProviderSettings)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _restApiConfigurationProviderSettings = configurationProviderSettings ?? throw new ArgumentNullException(nameof(configurationProviderSettings));
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _restApiHttpClient = restApiHttpClient;

        _logger = loggerFactory.CreateLogger<RestApiConfigurationSource>();

        _logger.LogTrace($"Creating new {nameof(RestApiConfigurationSource)}.");
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (_configurationProvider != null)
        {
            return _configurationProvider;
        }

        _logger.LogTrace($"{nameof(RestApiConfigurationSource)} builds provider instance.");

        var configurationRestApiClient = new ConfigurationRestApiClient(_restApiHttpClient, _loggerFactory);

        _configurationProvider = new RestApiConfigurationProvider(
            configurationRestApiClient, _distributedCache, _restApiConfigurationProviderSettings, _logger);

        return _configurationProvider;
    }
}
