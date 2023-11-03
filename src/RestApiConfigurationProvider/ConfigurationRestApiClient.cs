using System;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestApiConfigurationProvider.Exceptions;
using RestApiConfigurationProvider.HttpClients;
using RestApiConfigurationProvider.HttpClients.DTO.Requests;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;

namespace RestApiConfigurationProvider;

/// <summary>
/// Client for managing configurations in RestApiConfigurationProvider.
/// </summary>
public class ConfigurationRestApiClient : IConfigurationRestApiClient
{
    private readonly IRestApiHttpClient _restApiHttpClient;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationRestApiClient"/> class.
    /// </summary>
    /// <param name="restApiHttpClient">HTTP client interacting with RestApiConfigurationProvider.</param>
    /// <param name="loggerFactory">Factory to create logger.</param>
    public ConfigurationRestApiClient(IRestApiHttpClient restApiHttpClient, ILoggerFactory loggerFactory)
    {
        _restApiHttpClient = restApiHttpClient;
        _logger = loggerFactory.CreateLogger<ConfigurationRestApiClient>();
    }

    /// <summary>
    /// Inserts or updates the configuration asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of the configuration object.</typeparam>
    /// <param name="key">Unique identifier of the configuration.</param>
    /// <param name="obj">Configuration object.</param>
    /// <param name="cacheDurationSeconds">Duration to cache the configuration.</param>
    /// <param name="comments">Comments related to the changes.</param>
    /// <param name="lastChangedBy">Identifier of the last user/process that changed the configuration.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpsertConfigurationAsync<T>(string key, T obj, int cacheDurationSeconds = 0, string comments = null, string lastChangedBy = null)
    {
        _logger.LogTrace($"Calling {nameof(UpsertConfigurationAsync)} and parameters, ConfigurationSetId={key}, CacheDurationSeconds={cacheDurationSeconds}");

        var configurationChangeRequest = new ConfigurationsChangeRequest
        {
            // Duration config value will cached during GetConfigItem
            CacheDurationSeconds = cacheDurationSeconds,
            ContentType = MediaTypeNames.Application.Json,
            Comments = comments,
            LastChangedBy = lastChangedBy ?? Assembly.GetExecutingAssembly().GetName().Name
        };

        try
        {
            _logger.LogDebug($"Serialize config object to JSON. ConfigurationSetId={key}");
            configurationChangeRequest.ConfigurationValue = JsonConvert.SerializeObject(obj);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unable to serialize configuration object to JSON for ConfigurationSetId={key}";
            _logger.LogError(errorMessage);
            throw new JsonSerializationException(errorMessage, ex);
        }

        try
        {
            await _restApiHttpClient.UpsertConfigurationAsync(key, configurationChangeRequest);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unable to upsert configuration with ConfigurationSetId={key}";
            throw new ConfigurationException(errorMessage, ex);
        }
    }

    /// <summary>
    /// Retrieves the configuration asynchronously.
    /// </summary>
    /// <param name="key">Unique identifier of the configuration.</param>
    /// <param name="throwIfNull">Throws an exception if configuration is not found.</param>
    /// <returns>A task returning the retrieved configuration.</returns>
    public virtual async Task<ConfigurationResponse> GetConfigurationAsync(string key, bool throwIfNull = false)
    {
        _logger.LogTrace($"Calling {nameof(GetConfigurationAsync)} and parameters, ConfigurationSetId={key}");

        return await GetConfigurationInternalAsync(key, throwIfNull);
    }

    /// <summary>
    /// Retrieves and deserializes the configuration value asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of the configuration value object.</typeparam>
    /// <param name="key">Unique identifier of the configuration.</param>
    /// <returns>A task returning the deserialized configuration value.</returns>
    public virtual async Task<T> GetConfigurationValueAsync<T>(string key)
    {
        var configurationResponse = await GetConfigurationInternalAsync(key, true);
        try
        {
            return JsonConvert.DeserializeObject<T>(configurationResponse.ConfigurationValue);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unable to deserialize configuration item from JSON for ConfigurationSetId={key}";

            throw new ConfigurationException(errorMessage, ex);
        }
    }

    private async Task<ConfigurationResponse> GetConfigurationInternalAsync(string key, bool throwIfNull = false)
    {
        ConfigurationResponse configurationResponse;

        try
        {
            configurationResponse = await _restApiHttpClient.GetCurrentConfigurationAsync(key);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unable to get configuration with ConfigurationSetId={key}";
            throw new ConfigurationException(errorMessage, ex);
        }

        if (configurationResponse == null && throwIfNull)
        {
            var errorMessage = $"Unable to find configuration for ConfigurationSetId={key}";
            _logger.LogError(errorMessage);
            throw new ConfigurationException(errorMessage);
        }

        return configurationResponse;
    }
}
