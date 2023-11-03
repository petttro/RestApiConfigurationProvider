using System;

namespace RestApiConfigurationProvider.HttpClients.Config;

/// <summary>
/// Configuration settings necessary for the operation of the ConfigurationRestApiClient.
/// </summary>
public class ConfigurationHttpClientSettings
{
    /// <summary>
    /// Represents the section name in the configuration.
    /// </summary>
    public static string SectionName = "ConfigurationHttpClientSettings";

    /// <summary>
    /// Gets or sets the base URL for the Configuration API.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the API key used for authentication with the Configuration REST API.
    /// </summary>
    public Guid ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the HTTP timeout for requests to the Configuration REST API, in seconds. Default is 10 seconds.
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 10;
}
