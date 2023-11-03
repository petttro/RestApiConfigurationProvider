using System.Net;
using System.Net.Http.Headers;
using Moq;
using RestApiConfigurationProvider.HttpClients;
using RestApiConfigurationProvider.HttpClients.Config;
using RestApiConfigurationProvider.HttpClients.DelegatingHeaders;
using RestApiConfigurationProvider.HttpClients.DTO.Requests;
using RestApiConfigurationProvider.HttpClients.DTO.Responses;
using Xunit;

namespace RestApiConfigurationProvider.Test.HttpClients.DelegatingHandlers;

public class AuthHeaderHandlerTests : MockStrictBehaviorTest
{
    private readonly AuthHeaderHandler _authHeaderHandler;
    private const string _authorizationToken = "Token";

    private readonly ConfigurationHttpClientSettings _configurationHttpClientSettings = new ConfigurationHttpClientSettings()
    {
        ApiKey = Guid.NewGuid(),
        BaseUrl = "BaseUrl"
    };

    private readonly Mock<IRestApiAuthHttpClient> _authorizationClientMock;

    private readonly HttpRequestMessage _httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");

    public AuthHeaderHandlerTests()
    {
        _httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer");
        _authorizationClientMock = _mockRepository.Create<IRestApiAuthHttpClient>();

        _authHeaderHandler = new AuthHeaderHandler(_authorizationClientMock.Object, _configurationHttpClientSettings)
        {
            InnerHandler = new FakeHandler()
        };
    }

    [Fact]
    public async Task SendAsync_TokenNotExpired()
    {
        _authorizationClientMock
            .Setup(x => x.CreateClientToken(It.IsAny<ClientAuthRequest>()))
            .ReturnsAsync(new ClientAuthenticationResponse
            {
                Token = _authorizationToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

        var invoker = new HttpMessageInvoker(_authHeaderHandler);
        var result = await invoker.SendAsync(_httpRequestMessage, CancellationToken.None);

        Assert.Contains(_authorizationToken, result.RequestMessage?.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task SendAsync_TokenExpired()
    {
        _authorizationClientMock
            .Setup(x => x.CreateClientToken(It.IsAny<ClientAuthRequest>()))
            .ReturnsAsync(new ClientAuthenticationResponse
            {
                Token = _authorizationToken,
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            });

        var invoker = new HttpMessageInvoker(_authHeaderHandler);
        var result = await invoker.SendAsync(_httpRequestMessage, CancellationToken.None);

        Assert.Contains(_authorizationToken, result.RequestMessage?.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task SendAsync_NoToken()
    {
        _authorizationClientMock
            .Setup(x => x.CreateClientToken(It.IsAny<ClientAuthRequest>()))
            .ReturnsAsync(new ClientAuthenticationResponse()
            {
                Token = _authorizationToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });

        var invoker = new HttpMessageInvoker(_authHeaderHandler);
        var result = await invoker.SendAsync(_httpRequestMessage, CancellationToken.None);

        Assert.Contains(_authorizationToken, result.RequestMessage?.Headers.Authorization?.ToString());
    }

    private class FakeHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request }, cancellationToken);
        }
    }
}
