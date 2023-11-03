using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RestApiConfigurationProvider.ConfigurationProviders;

namespace RestApiConfigurationProvider.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IConfiguration"/> to support asynchronous loading
/// and cache invalidation for REST API configuration providers.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Asynchronously loads configuration data from all Rest Api configuration providers in the configuration root.
    /// </summary>
    /// <param name="configuration">The configuration from which to load data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task LoadAsync(this IConfiguration configuration)
    {
        if (configuration is not IConfigurationRoot configurationRoot)
        {
            return;
        }

        foreach (IConfigurationProvider provider in configurationRoot.Providers)
        {
            if (provider is IRestApiConfigurationProvider configurationProvider)
            {
                await configurationProvider.LoadAsync();
            }
        }
    }

    /// <summary>
    /// Invalidates the cache for a specific configuration set in configuration provider in the configuration root.
    /// </summary>
    /// <param name="configuration">The configuration for which to invalidate the cache.</param>
    /// <param name="configurationSetId">The ID of the configuration set for which to invalidate the cache.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task InvalidateCacheAsync(this IConfiguration configuration, string configurationSetId)
    {
        if (configuration is not IConfigurationRoot configurationRoot)
        {
            return;
        }

        foreach (IConfigurationProvider provider in configurationRoot.Providers)
        {
            if (provider is IRestApiConfigurationProvider configurationProvider)
            {
                await configurationProvider.InvalidateCacheAsync(configurationSetId);
            }
        }
    }
}
