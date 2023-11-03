using System.ComponentModel.DataAnnotations;

namespace RestApiConfigurationProvider.HttpClients.DTO.Requests;

/// <summary>
/// Represents a request to retrieve configuration details.
/// </summary>
public class ConfigurationsGetRequest
{
    /// <summary>
    /// Gets or sets the identifier of the configuration set to be retrieved.
    /// </summary>
    /// <value>
    /// A string representing the unique identifier of the configuration set.
    /// </value>
    /// <remarks>
    /// This property is required for the request.
    /// </remarks>
    [Required]
    public string ConfigurationSetId { get; set; }

    /// <summary>
    /// Gets or sets the application name for which the configuration details are to be retrieved.
    /// </summary>
    /// <value>
    /// A string representing the name of the application.
    /// </value>
    public string Application { get; set; }
}
