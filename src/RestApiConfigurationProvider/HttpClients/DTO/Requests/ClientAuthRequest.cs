using System;
using System.ComponentModel.DataAnnotations;

namespace RestApiConfigurationProvider.HttpClients.DTO.Requests;

/// <summary>
/// Represents a request for client authentication.
/// </summary>
public class ClientAuthRequest
{
    /// <summary>
    /// Gets or sets the unique key required for client authentication.
    /// </summary>
    /// <remarks>
    /// This key is necessary for the authentication process, and it must be provided in the request.
    /// </remarks>
    /// <value>
    /// A nullable <see cref="Guid"/> representing the unique authentication key.
    /// </value>
    [Required(ErrorMessage = "The Key is required for client authentication.")]
    public Guid? Key { get; set; }
}
