using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestApiConfigurationProvider.ConfigurationProviders;
using RestApiConfigurationProvider.HttpClients;

namespace RestApiConfigurationProvider.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IConfigurationBuilder"/> to support adding RestApiConfigurationProvider configuration sources.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds a REST API configuration source to the configuration builder. This extension method allows
    /// the application to retrieve configuration settings from the Configuration REST API service.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to which the configuration source is added.</param>
    /// <param name="loggerFactory">Used for logging during the configuration retrieval process.</param>
    /// <param name="distributedCache">A distributed cache for temporarily storing fetched configuration.</param>
    /// <param name="configurationRestApiClient">An HTTP client for interacting with the configuration rest api service.</param>
    /// <param name="restApiConfigurationProviderSettings">Settings required by the rest api configuration provider.</param>
    /// <returns>The updated <see cref="IConfigurationBuilder"/> with the rest api configuration source added.</returns>
    public static IConfigurationBuilder AddRestApiConfigurationSource(
        this IConfigurationBuilder configurationBuilder,
        ILoggerFactory loggerFactory,
        IDistributedCache distributedCache,
        IConfigurationRestApiClient configurationRestApiClient,
        RestApiConfigurationProviderSettings restApiConfigurationProviderSettings)
    {
        var source = new RestApiConfigurationSource(loggerFactory, distributedCache, configurationRestApiClient, restApiConfigurationProviderSettings);
        configurationBuilder.Add(source);

        return configurationBuilder;
    }
}
