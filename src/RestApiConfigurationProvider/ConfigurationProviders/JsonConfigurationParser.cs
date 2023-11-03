using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

[assembly: InternalsVisibleTo("RestApiConfigurationProvider.Test")]

namespace RestApiConfigurationProvider.ConfigurationProviders;

/// <summary>
/// Slightly modified. See Microsoft.Extensions.Configuration.Json.JsonConfigurationFileParser.
/// </summary>
internal class JsonConfigurationParser
{
    private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> _context = new Stack<string>();
    private string _currentPath;
    private string _section;

    private JsonConfigurationParser()
    {
    }

    public static IDictionary<string, string> Parse(string input, string section = null) => new JsonConfigurationParser().ParseStream(input, section);

    private IDictionary<string, string> ParseStream(string input, string section)
    {
        _data.Clear();

        _section = section;

        var token = JToken.Parse(input);
        VisitToken(token);

        return _data;
    }

    private void VisitJObject(JObject jObject)
    {
        foreach (var property in jObject.Properties())
        {
            EnterContext(property.Name);
            VisitProperty(property);
            ExitContext();
        }
    }

    private void VisitProperty(JProperty property)
    {
        VisitToken(property.Value);
    }

    private void VisitToken(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                VisitJObject(token.Value<JObject>());
                break;

            case JTokenType.Array:
                VisitArray(token.Value<JArray>());
                break;

            case JTokenType.Integer:
            case JTokenType.Float:
            case JTokenType.String:
            case JTokenType.Boolean:
            case JTokenType.Bytes:
            case JTokenType.Raw:
            case JTokenType.Null:
            case JTokenType.Date:
            case JTokenType.Guid:
            case JTokenType.Uri:
            case JTokenType.TimeSpan:
                VisitPrimitive(token.Value<JValue>());
                break;

            default:
                throw new FormatException($"Unsupported JSON token Type={token.Type}, Path={token.Path}");
        }
    }

    private void VisitArray(JArray array)
    {
        for (int index = 0; index < array.Count; index++)
        {
            EnterContext(index.ToString());
            VisitToken(array[index]);
            ExitContext();
        }
    }

    private void VisitPrimitive(JValue data)
    {
        var key = string.IsNullOrWhiteSpace(_section) ? _currentPath : $"{_section}:{_currentPath}";

        if (_data.ContainsKey(key))
        {
            throw new FormatException($"Key={key} is duplicated");
        }

        _data[key] = data.ToString(CultureInfo.InvariantCulture);
    }

    private void EnterContext(string context)
    {
        _context.Push(context);
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }

    private void ExitContext()
    {
        _context.Pop();
        _currentPath = ConfigurationPath.Combine(_context.Reverse());
    }
}
