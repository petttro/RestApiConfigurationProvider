using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using RestApiConfigurationProvider.HttpClients.Config;
using RestApiConfigurationProvider.HttpClients.DTO.Requests;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;

namespace RestApiConfigurationProvider.HttpClients.DelegatingHeaders;

internal class AuthHeaderHandler : DelegatingHandler
{
    private readonly IRestApiAuthHttpClient _authorizationClient;
    private readonly ConfigurationHttpClientSettings _configurationHttpClientSettings;
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private ClientAuthenticationResponse _clientToken;

    public AuthHeaderHandler(IRestApiAuthHttpClient authorizationClient, ConfigurationHttpClientSettings options)
    {
        _authorizationClient = authorizationClient;
        _configurationHttpClientSettings = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            if (_clientToken == null || _clientToken.ExpiresAt <= DateTime.UtcNow)
            {
                _clientToken = await _authorizationClient.CreateClientToken(new ClientAuthRequest { Key = _configurationHttpClientSettings.ApiKey });
            }
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _clientToken.Token);

        return await base.SendAsync(request, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _semaphoreSlim?.Dispose();
        }

        base.Dispose(disposing);
    }
}
