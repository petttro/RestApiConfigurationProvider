using System.Threading.Tasks;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;

namespace RestApiConfigurationProvider;

/// <summary>
/// Interface for managing configurations in Configuration Rest Api service.
/// </summary>
public interface IConfigurationRestApiClient
{
    /// <summary>
    /// Inserts or updates a configuration in configuration service.
    /// </summary>
    /// <typeparam name="T">The type of the configuration object.</typeparam>
    /// <param name="key">Unique identifier of the configuration.</param>
    /// <param name="obj">The configuration object to upsert.</param>
    /// <param name="cacheDurationSeconds">Optional. Duration to cache the configuration in seconds. Default is 0.</param>
    /// <param name="comments">Optional. Comments related to the changes.</param>
    /// <param name="lastChangedBy">Optional. Identifier of the last user/process that changed the configuration.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task UpsertConfigurationAsync<T>(string key, T obj, int cacheDurationSeconds = 0, string comments = null, string lastChangedBy = null);

    /// <summary>
    /// Retrieves a configuration from Configuration Rest Api Service.
    /// </summary>
    /// <param name="key">Unique identifier of the configuration.</param>
    /// <param name="throwIfNull">Optional. A flag determining whether to throw an exception if the configuration is not found. Default is false.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns the retrieved configuration.</returns>
    Task<ConfigurationResponse> GetConfigurationAsync(string key, bool throwIfNull = false);

    /// <summary>
    /// Retrieves and deserializes the value of a configuration from Configuration Rest Api Service.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the configuration value into.</typeparam>
    /// <param name="key">Unique identifier of the configuration.</param>
    /// <returns>A <see cref="Task{TResult}"/> that returns the deserialized configuration value.</returns>
    Task<T> GetConfigurationValueAsync<T>(string key);
}
