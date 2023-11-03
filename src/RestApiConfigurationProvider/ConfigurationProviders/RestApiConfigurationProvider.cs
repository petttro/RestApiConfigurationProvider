using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestApiConfigurationProvider.Extensions;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;

namespace RestApiConfigurationProvider.ConfigurationProviders;

internal class RestApiConfigurationProvider : ConfigurationProvider, IRestApiConfigurationProvider
{
    private readonly IConfigurationRestApiClient _configurationRestApiClient;
    private readonly IDistributedCache _distributedCache;
    private readonly RestApiConfigurationProviderSettings _restApiConfigurationProviderSettings;
    private readonly ILogger _logger;

    /// <summary>
    /// This structure allows us to update data for a specific configuration in one atomic operation instead of having to do a bunch of
    /// "sets" and "removes" on one global dictionary
    /// <see cref="ConfigurationProvider.Data"/> is an internal interface for whenever we need to work with it as with one "whole" Dictionary as it is done in
    /// <see cref="ConfigurationProvider"/>.
    /// </summary>
    private readonly IDictionary<string, IDictionary<string, string>> _configurationCache = new ConcurrentDictionary<string, IDictionary<string, string>>();

    public RestApiConfigurationProvider(
        IConfigurationRestApiClient configurationRestApiClient,
        IDistributedCache distributedCache,
        RestApiConfigurationProviderSettings restApiConfigurationProviderSettings,
        ILogger logger)
    {
        _configurationRestApiClient = configurationRestApiClient;
        _distributedCache = distributedCache;
        _restApiConfigurationProviderSettings = restApiConfigurationProviderSettings ??
                                                    throw new ArgumentNullException(nameof(restApiConfigurationProviderSettings));
        _logger = logger;
    }

    public override void Load()
    {
        try
        {
            LoadAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while loading configuration");
        }
    }

    public async Task LoadAsync()
    {
        var configTasks = _restApiConfigurationProviderSettings.ConfigurationKeys.Select(LoadConfigurationAsync).ToArray();

        await Task.WhenAll(configTasks);

        Data = _configurationCache.Merge(StringComparer.OrdinalIgnoreCase);

        OnReload();
    }

    public async Task InvalidateCacheAsync(string configurationKey)
    {
        var cacheKey = BuildCacheKey(configurationKey);
        await _distributedCache.RemoveAsync(cacheKey);
    }

    private async Task LoadConfigurationAsync(string configurationKey)
    {
        var cacheKey = BuildCacheKey(configurationKey);

        // Attempting to get the configurationValue from the cache.
        var configurationValue = await _distributedCache.GetAsync<IDictionary<string, string>>(cacheKey);

        // If configurationValue is null, get it from REST API and set to distributed cache.
        if (configurationValue == null)
        {
            _logger.LogWarning($"Configuration Data not found in Distributed Cache for CacheKey={cacheKey}");

            configurationValue = await GetFromRestApiAsync(configurationKey);
            if (configurationValue != null)
            {
                await SetToCacheAsync(configurationKey, configurationValue);
            }
        }

        // Set to local cache
        if (configurationValue != null)
        {
            _configurationCache[configurationKey] = configurationValue;
        }
    }

    private async Task SetToCacheAsync(string configurationKey, IDictionary<string, string> configurationValue)
    {
        var cacheKey = BuildCacheKey(configurationKey);
        var cacheDurationMinutes = _restApiConfigurationProviderSettings.CacheDurationMinutes;
        await _distributedCache.SetAsync(cacheKey, configurationValue, TimeSpan.FromMinutes(cacheDurationMinutes));

        _logger.LogInformation($"Set Configuration Data to Distributed Cache CacheKey={cacheKey}, " +
                               $"ConfigDictionary={configurationValue.SerializeJsonSafe()}, DistributedCacheDurationMinutes={cacheDurationMinutes}");
    }

    private async Task<IDictionary<string, string>> GetFromRestApiAsync(string configurationSetId)
    {
        _logger.LogTrace($"Loading ConfigurationSetId={configurationSetId} from Configuration REST API.");

        ConfigurationResponse config = null;
        try
        {
            config = await _configurationRestApiClient.GetConfigurationAsync(configurationSetId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unable to load configuration from Configuration REST API for ConfigurationSetId={configurationSetId}");
        }

        if (config == null)
        {
            return null;
        }

        try
        {
            var result = JsonConfigurationParser.Parse(config.ConfigurationValue, config.ConfigurationSetId);
            _logger.LogTrace($"Successfully deserialized ConfigurationSetId={configurationSetId} from json");

            return result;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unable to deserialize configuration. Please make sure it's a valid JSON ASAP. ConfigurationSetId={config.ConfigurationSetId}, " +
                               $"LastChangedBy={config.LastChangedBy}, LastUpdateDateTime={config.LastUpdateDateTime}.";
            _logger.LogError(ex, errorMessage);
        }

        return null;
    }

    private string BuildCacheKey(string configurationKey)
    {
        return $"{_restApiConfigurationProviderSettings.CacheKeyPrefix}_{configurationKey}";
    }
}
