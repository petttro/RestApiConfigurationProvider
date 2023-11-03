using System;

namespace RestApiConfigurationProvider.HttpClients.DTO.Responses;

/// <summary>
/// Represents the response received after client authentication.
/// </summary>
public class ClientAuthenticationResponse
{
    /// <summary>
    /// Gets or sets the name of the application that the client has authenticated against.
    /// </summary>
    /// <value>
    /// A string representing the name of the application.
    /// </value>
    public string Application { get; set; }

    /// <summary>
    /// Gets or sets the authentication JWT token generated after successful client authentication.
    /// </summary>
    /// <value>
    /// A string representing the authentication JWT token.
    /// </value>
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time of the authentication token.
    /// </summary>
    /// <value>
    /// A DateTime object representing the expiration date and time of the token.
    /// </value>
    public DateTime ExpiresAt { get; set; }
}
