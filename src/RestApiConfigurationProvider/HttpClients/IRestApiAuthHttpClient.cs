using System.Threading.Tasks;
using Refit;
using RestApiConfigurationProvider.HttpClients.DTO.Requests;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;

namespace RestApiConfigurationProvider.HttpClients;

/// <summary>
/// Interface for the Http Client focused on authentication in Configuration Rest Api service, defined using Refit.
/// </summary>
[Headers("Accept: application/json", "Content-Type: application/json")]
public interface IRestApiAuthHttpClient
{
    /// <summary>
    /// Creates a client token for authentication by making a POST request with the provided ClientTokenRequest object.
    /// </summary>
    /// <param name="clientTokenRequest">An object containing necessary data to request a client token.</param>
    /// <returns>A Task representing the asynchronous operation, containing the ClientTokenResponse.</returns>
    [Post("/v1/auth/client")]
    Task<ClientAuthenticationResponse> CreateClientToken(ClientAuthRequest clientTokenRequest);
}
