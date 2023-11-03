using System.Threading.Tasks;
using Refit;
using RestApiConfigurationProvider.HttpClients.DTO.Requests;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;

namespace RestApiConfigurationProvider.HttpClients;

/// <summary>
/// Interface for Configuration REST HTTP Client interacting, defined using Refit.
/// </summary>
[Headers("Accept: application/json", "Content-Type: application/json", "Authorization: Bearer")]
public interface IRestApiHttpClient
{
    /// <summary>
    /// Retrieves the current configuration for a specified configuration set ID.
    /// </summary>
    /// <param name="configurationSetId">The ID of the configuration set to retrieve.</param>
    /// <returns>A Task representing the asynchronous operation, containing the ConfigurationResponse.</returns>
    [Get("/v1/configurations/{configurationSetId}/current")]
    Task<ConfigurationResponse> GetCurrentConfigurationAsync(string configurationSetId);

    /// <summary>
    /// Inserts or updates a configuration based on the provided ConfigurationChangeRequest.
    /// </summary>
    /// <param name="configurationSetId">The ID of the configuration set to create/update.</param>
    /// <param name="configurationChangeRequest">The request object containing necessary data for inserting or updating the configuration.</param>
    /// <returns>A Task representing the asynchronous operation, containing the ConfigurationChangeResponse.</returns>
    [Post("/v1/configurations/{configurationSetId}")]
    Task<ConfigurationChangeResponse> UpsertConfigurationAsync(string configurationSetId, ConfigurationsChangeRequest configurationChangeRequest);
}
