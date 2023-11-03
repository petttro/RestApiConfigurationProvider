using System;

namespace RestApiConfigurationProvider.HttpClients.DTO.Responses;

/// <summary>
/// Represents the response received when querying for configuration information.
/// </summary>
public class ConfigurationResponse
{
    /// <summary>
    /// Gets or sets the identifier of the configuration set.
    /// </summary>
    public string ConfigurationSetId { get; set; }

    /// <summary>
    /// Gets or sets the application associated with the configuration.
    /// </summary>
    public string Application { get; set; }

    /// <summary>
    /// Gets or sets the version of the configuration.
    /// </summary>
    public int ConfigurationVersion { get; set; }

    /// <summary>
    /// Gets or sets the actual value of the configuration.
    /// </summary>
    public string ConfigurationValue { get; set; }

    /// <summary>
    /// Gets or sets the last update date and time of the configuration.
    /// </summary>
    public DateTime LastUpdateDateTime { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last changed the configuration.
    /// </summary>
    public string LastChangedBy { get; set; }

    /// <summary>
    /// Gets or sets comments or notes associated with the configuration change.
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    /// Gets or sets the content type of the configuration value, if applicable.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the duration, in seconds, for which the configuration value should be cached.
    /// </summary>
    public int CacheDurationSeconds { get; set; }
}
