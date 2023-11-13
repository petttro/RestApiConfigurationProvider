using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RestApiConfigurationProvider.ConfigurationProviders;

internal class RestApiConfigurationSource : IConfigurationSource
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDistributedCache _distributedCache;
    private readonly IConfigurationRestApiClient _configurationRestApiClient;
    private readonly RestApiConfigurationProviderSettings _restApiConfigurationProviderSettings;

    private readonly ILogger _logger;
    private IConfigurationProvider _configurationProvider;

    public RestApiConfigurationSource(
        ILoggerFactory loggerFactory,
        IDistributedCache distributedCache,
        IConfigurationRestApiClient configurationRestApiClient,
        RestApiConfigurationProviderSettings configurationProviderSettings)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _restApiConfigurationProviderSettings = configurationProviderSettings ?? throw new ArgumentNullException(nameof(configurationProviderSettings));
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _configurationRestApiClient = configurationRestApiClient;

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

        _configurationProvider = new RestApiConfigurationProvider(
            _configurationRestApiClient, _distributedCache, _restApiConfigurationProviderSettings, _logger);

        return _configurationProvider;
    }
}
