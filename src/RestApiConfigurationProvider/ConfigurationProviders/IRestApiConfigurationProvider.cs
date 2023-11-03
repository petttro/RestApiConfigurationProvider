using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RestApiConfigurationProvider.ConfigurationProviders;

/// <summary>
/// Interface for providing configuration settings from Configuration Rest Api servicce with additional capabilities such as cache invalidation.
/// </summary>
public interface IRestApiConfigurationProvider : IConfigurationProvider
{
    /// <summary>
    /// Asynchronously invalidates the cache for a specific configuration key.
    /// </summary>
    /// <param name="configurationKey">The key of the configuration setting to invalidate.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task InvalidateCacheAsync(string configurationKey);

    /// <summary>
    /// Asynchronously loads configuration settings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task LoadAsync();
}
