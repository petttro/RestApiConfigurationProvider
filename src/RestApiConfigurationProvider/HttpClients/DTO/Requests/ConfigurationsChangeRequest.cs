using System.ComponentModel.DataAnnotations;

namespace RestApiConfigurationProvider.HttpClients.DTO.Requests;

/// <summary>
/// Represents a request to change configurations.
/// </summary>
public class ConfigurationsChangeRequest
{
    /// <summary>
    /// Gets or sets the configuration value that is required for updating the configurations.
    /// </summary>
    /// <value>
    /// A string representing the configuration value.
    /// </value>
    [Required(ErrorMessage = "Configuration value is required.")]
    public string ConfigurationValue { get; set; }

    /// <summary>
    /// Gets or sets the comments that provide additional context or information about the configuration change.
    /// </summary>
    /// <value>
    /// A string representing comments about the configuration change.
    /// </value>
    [Required(ErrorMessage = "Comments are required.")]
    public string Comments { get; set; }

    /// <summary>
    /// Gets or sets the content type of the configuration value, if applicable.
    /// </summary>
    /// <value>
    /// A string representing the content type.
    /// </value>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the duration for which the configuration should be cached, in seconds.
    /// </summary>
    /// <value>
    /// An integer representing the cache duration in seconds.
    /// </value>
    public int CacheDurationSeconds { get; set; } = 0;

    /// <summary>
    /// Gets or sets the identifier of the user who made the last change to the configuration.
    /// </summary>
    /// <value>
    /// A string representing the identifier of the user who last changed the configuration.
    /// </value>
    [Required(ErrorMessage = "Last changed by information is required.")]
    public string LastChangedBy { get; set; }
}
