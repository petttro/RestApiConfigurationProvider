using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestApiConfigurationProvider.HttpClients.Extensions;

namespace RestApiConfigurationProvider.HttpClients.DelegatingHeaders;

internal class LogResponseHandler : DelegatingHandler
{
    private readonly ILogger<LogResponseHandler> _logger;

    public LogResponseHandler(ILogger<LogResponseHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string requestBody = null;
        if (request.Content != null)
        {
            requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        var requestVerb = request.Method.Method;
        var requestUrl = request.RequestUri;
        var requestHeadersString = Extensions.LoggerExtensions.GetHeaderTraceString(request.Headers).ReplaceLineBreaksWithSpaces();
        var requestEditedBody = requestBody.ReplaceLineBreaksWithSpaces();

        _logger.LogInformation($"WebRequest: Verb={requestVerb}, Url={requestUrl}, Headers=[{requestHeadersString}], Body='{requestEditedBody}'");

        var stopwatch = Stopwatch.StartNew();

        var response = await base.SendAsync(request, cancellationToken);
        await response.Content.LoadIntoBufferAsync();

        stopwatch.Stop();

        IEnumerable<string> acceptEncoding = null;
        response.RequestMessage?.Headers.TryGetValues("Accept-Encoding", out acceptEncoding);
        var isBinaryResponse = acceptEncoding?.Contains("gzip") ?? false;

        var responseBody = "[ZIPPED BINARY CONTENT]";
        if (!isBinaryResponse)
        {
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        }

        var responseVerb = response.RequestMessage?.Method.Method;
        var responseUrl = response.RequestMessage?.RequestUri;
        var responseHeadersString = response.Headers.GetHeaderTraceString().ReplaceLineBreaksWithSpaces();
        var responseEditedBody = responseBody.ReplaceLineBreaksWithSpaces();

        var logMsg = $"WebResponse: Status={(int)response.StatusCode}, Verb={responseVerb}, Url={responseUrl}, " +
                     $"Duration={stopwatch.ElapsedMilliseconds}, Headers=[{responseHeadersString}], Body='{responseEditedBody}'";
        _logger.Log(response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Error, logMsg);

        return response;
    }
}
