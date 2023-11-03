using System;

namespace RestApiConfigurationProvider.HttpClients.DTO.Responses;

/// <summary>
/// Represents the response received after a configuration change request.
/// </summary>
public class ConfigurationChangeResponse
{
    /// <summary>
    /// Gets or sets the date and time of the last update to the configuration.
    /// </summary>
    /// <value>
    /// A DateTime object representing the last update date and time of the configuration.
    /// </value>
    public DateTime LastUpdateDateTime { get; set; }

    /// <summary>
    /// Gets or sets the version of the configuration after the change.
    /// </summary>
    /// <value>
    /// An integer representing the version of the configuration.
    /// </value>
    public int ConfigurationVersion { get; set; }
}
