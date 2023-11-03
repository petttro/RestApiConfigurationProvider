using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestApiConfigurationProvider.HttpClients.DelegatingHeaders;
using Xunit;

namespace RestApiConfigurationProvider.Test.HttpClients.DelegatingHandlers;

public class LogResponseHandlerTests : MockStrictBehaviorTest
{
    private readonly LogResponseHandler _logResponseHandler;

    private readonly HttpRequestMessage _httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
    private readonly Mock<ILogger<LogResponseHandler>> _loggerMock;

    public LogResponseHandlerTests()
    {
        _httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer");
        _httpRequestMessage.RequestUri = new Uri("https://example.com");
        _httpRequestMessage.Method = HttpMethod.Post;
        _httpRequestMessage.Content = new StringContent("Request content body");

        _loggerMock = _mockRepository.Create<ILogger<LogResponseHandler>>();

        _logResponseHandler = new LogResponseHandler(new NullLogger<LogResponseHandler>())
        {
            InnerHandler = new FakeHandler()
        };
    }

    [Fact]
    public async Task SendAsync_LoggedResponse()
    {
        var invoker = new HttpMessageInvoker(_logResponseHandler);
        var result = await invoker.SendAsync(_httpRequestMessage, CancellationToken.None);

    }

    private class FakeHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request }, cancellationToken);
        }
    }
}
