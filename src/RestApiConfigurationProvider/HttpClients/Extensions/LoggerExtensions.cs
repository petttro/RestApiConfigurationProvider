using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace RestApiConfigurationProvider.HttpClients.Extensions;

internal static class LoggerExtensions
{
    // TODO: make this configurable
    private const int _maxHeaderLength = 1000;

    private static string GetHeaderTraceString(this HttpHeaders headers)
    {
        if (headers == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var header in headers)
        {
            builder.AppendLine($"{header.Key}='{(header.Value.Any() ? string.Join(",", header.Value) : string.Empty).Limit(_maxHeaderLength)}'");
        }

        return builder.ToString();
    }

    public static string GetHeaderTraceString(this HttpResponseHeaders headers)
    {
        return (headers as HttpHeaders).GetHeaderTraceString();
    }

    public static string GetHeaderTraceString(HttpRequestHeaders headers)
    {
        return (headers as HttpHeaders).GetHeaderTraceString();
    }

    public static string Limit(this string input, int maxSize)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return input.Length <= maxSize
            ? input
            : input.Substring(0, maxSize) + " ***** LIMITED TO " + maxSize + " BYTES *****";
    }

    public static string ReplaceLineBreaksWithSpaces(this string str)
    {
        return string.IsNullOrEmpty(str) ? str : Regex.Replace(str, @"\s+", " ");
    }
}
