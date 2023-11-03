using RestApiConfigurationProvider.ConfigurationProviders;
using Xunit;

namespace RestApiConfigurationProvider.Test.ConfigurationProvider;

public class JsonConfigurationParserTests
{
    [Fact]
    public void Parse_ValidJsonObject_ParsesSuccessfully()
    {
        string json = @"{""key1"": ""value1"", ""key2"": ""value2""}";
        var result = JsonConfigurationParser.Parse(json);

        Assert.Equal("value1", result["key1"]);
        Assert.Equal("value2", result["key2"]);
    }

    [Fact]
    public void Parse_JsonWithNestedObject_ParsesSuccessfully()
    {
        string json = @"{""parent"": {""child"": ""value""}}";
        var result = JsonConfigurationParser.Parse(json);

        Assert.Equal("value", result["parent:child"]);
    }

    [Fact]
    public void Parse_JsonWithArray_ParsesSuccessfully()
    {
        string json = @"{""array"": [""value1"", ""value2""]}";
        var result = JsonConfigurationParser.Parse(json);

        Assert.Equal("value1", result["array:0"]);
        Assert.Equal("value2", result["array:1"]);
    }

    [Fact]
    public void Parse_JsonWithSpecialSection_ParsesSuccessfully()
    {
        string json = @"{""key"": ""value""}";
        string section = "section";
        var result = JsonConfigurationParser.Parse(json, section);

        Assert.Equal("value", result["section:key"]);
    }

    [Fact]
    public void Parse_UnsupportedJsonToken_ThrowsFormatException()
    {
        // Assuming that you're not supporting other types, adjust as necessary
        string json = @"//some comment {wrong json}";

        Assert.Throws<System.FormatException>(() => JsonConfigurationParser.Parse(json));
    }
}

