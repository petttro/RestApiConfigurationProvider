using System.Collections.Generic;

namespace RestApiConfigurationProvider.ConfigurationProviders;

/// <summary>
/// Represents settings for the RestApiConfigurationProvider.
/// </summary>
public class RestApiConfigurationProviderSettings
{
    /// <summary>
    /// The section name in the configuration file where
    /// RestApiConfigurationProvider settings are specified.
    /// </summary>
    public static string SectionName = "RestApiConfigurationProvider";

    /// <summary>
    /// Gets or sets the list of configuration keys that
    /// the RestApiConfigurationProvider will manage.
    /// </summary>
    public List<string> ConfigurationKeys { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the duration in minutes that configuration
    /// data will be cached by the RestApiConfigurationProvider.
    /// </summary>
    public int CacheDurationMinutes { get; set; }

    /// <summary>
    /// Gets or sets the prefix that will be used for cache keys
    /// managed by the RestApiConfigurationProvider.
    /// </summary>
    public string CacheKeyPrefix { get; set; }
}
