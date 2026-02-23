#pragma warning disable CA1707
namespace dotnet_config_kit.Tests.Parsers;

using dotnet_config_kit.Internal.Parsers;
using FluentAssertions;
using Xunit;

public sealed class JsonConfigParserTests
{
    private readonly JsonConfigParser _parser = new();

    [Fact]
    public void Parse_ValidJson_ReturnsFlatDictionary()
    {
        var json = """
            {
              "database": {
                "host": "localhost",
                "port": "5432"
              },
              "debug": true
            }
            """;

        var result = _parser.Parse(json);

        result.Should().ContainKeys("database.host", "database.port", "debug");
        result["database.host"].Should().Be("localhost");
        result["database.port"].Should().Be("5432");
        result["debug"].Should().Be("True");
    }

    [Fact]
    public void Parse_JsonWithArray_FlattenWithIndexes()
    {
        var json = """
            {
              "items": [
                { "name": "item1" },
                { "name": "item2" }
              ]
            }
            """;

        var result = _parser.Parse(json);

        result.Should().ContainKeys("items:0.name", "items:1.name");
        result["items:0.name"].Should().Be("item1");
        result["items:1.name"].Should().Be("item2");
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyDictionary()
    {
        var result = _parser.Parse("");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_EmptyJson_ReturnsEmptyDictionary()
    {
        var result = _parser.Parse("{}");

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_NullInput_ThrowsArgumentNullException()
    {
        var action = () => _parser.Parse(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsArgumentException()
    {
        var action = () => _parser.Parse("{invalid json}");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_JsonWithNullValue_StoresEmptyString()
    {
        var json = """
            {
              "nullable": null
            }
            """;

        var result = _parser.Parse(json);

        result.Should().ContainKey("nullable");
        result["nullable"].Should().Be("");
    }

    [Fact]
    public void Parse_NestedObjects_UsedDotNotation()
    {
        var json = """
            {
              "level1": {
                "level2": {
                  "level3": "value"
                }
              }
            }
            """;

        var result = _parser.Parse(json);

        result.Should().ContainKey("level1.level2.level3");
        result["level1.level2.level3"].Should().Be("value");
    }

    [Fact]
    public async Task ParseAsync_ValidJson_ReturnsFlatDictionary()
    {
        var json = """
            {
              "key": "value"
            }
            """;

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var result = await _parser.ParseAsync(stream, CancellationToken.None);

        result.Should().ContainKey("key");
        result["key"].Should().Be("value");
    }

    [Fact]
    public async Task ParseAsync_NullStream_ThrowsArgumentNullException()
    {
        var action = async () => await _parser.ParseAsync(null!, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Format_ReturnsJson()
    {
        _parser.Format.Should().Be("json");
    }
}
