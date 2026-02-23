#pragma warning disable CA1707
namespace dotnet_config_kit.Tests.Parsers;

using dotnet_config_kit.Internal.Parsers;
using FluentAssertions;
using Xunit;

public sealed class YamlConfigParserTests
{
    private readonly YamlConfigParser _parser = new();

    [Fact]
    public void Parse_ValidYaml_ReturnsFlatDictionary()
    {
        var yaml = """
            database:
              host: localhost
              port: 5432
              username: sa
            debug: true
            """;

        var result = _parser.Parse(yaml);

        result.Should().ContainKeys("database.host", "database.port", "database.username", "debug");
        result["database.host"].Should().Be("localhost");
        result["database.port"].Should().Be("5432");
        result["debug"].Should().Be("true");  // YAML preserves lowercase boolean
    }

    [Fact]
    public void Parse_YamlWithArray_FlattenWithIndexes()
    {
        var yaml = """
            servers:
              - name: server1
                port: 8080
              - name: server2
                port: 8081
            """;

        var result = _parser.Parse(yaml);

        result.Should().ContainKeys("servers:0.name", "servers:0.port", "servers:1.name", "servers:1.port");
        result["servers:0.name"].Should().Be("server1");
        result["servers:1.port"].Should().Be("8081");
    }

    [Fact]
    public void Format_ReturnsYaml()
    {
        _parser.Format.Should().Be("yaml");
    }
}

public sealed class TomlConfigParserTests
{
    private readonly TomlConfigParser _parser = new();

    [Fact]
    public void Parse_ValidToml_ReturnsFlatDictionary()
    {
        var toml = """
            [database]
            host = "localhost"
            port = 5432
            username = "sa"
            debug = true
            """;

        var result = _parser.Parse(toml);

        result.Should().ContainKeys("database.host", "database.port", "database.username", "database.debug");
        result["database.host"].Should().Be("localhost");
        result["database.port"].Should().Be("5432");
    }

    [Fact]
    public void Format_ReturnsToml()
    {
        _parser.Format.Should().Be("toml");
    }
}

public sealed class IniConfigParserTests
{
    private readonly IniConfigParser _parser = new();

    [Fact]
    public void Parse_ValidIni_ReturnsFlatDictionary()
    {
        var ini = """
            [database]
            host=localhost
            port=5432
            username=sa
            debug=true
            """;

        var result = _parser.Parse(ini);

        result.Should().ContainKeys("database.host", "database.port", "database.username", "database.debug");
        result["database.host"].Should().Be("localhost");
        result["database.port"].Should().Be("5432");
    }

    [Fact]
    public void Parse_QuotedValues_RemovesQuotes()
    {
        var ini = """
            name="My App"
            path='C:\config'
            """;

        var result = _parser.Parse(ini);

        result["name"].Should().Be("My App");
        result["path"].Should().Be("C:\\config");
    }

    [Fact]
    public void Format_ReturnsIni()
    {
        _parser.Format.Should().Be("ini");
    }
}

public sealed class XmlConfigParserTests
{
    private readonly XmlConfigParser _parser = new();

    [Fact]
    public void Parse_ValidXml_ReturnsFlatDictionary()
    {
        var xml = """
            <?xml version="1.0"?>
            <root>
              <database>
                <host>localhost</host>
                <port>5432</port>
              </database>
              <debug>true</debug>
            </root>
            """;

        var result = _parser.Parse(xml);

        result.Should().ContainKeys("root.database.host", "root.database.port", "root.debug");
        result["root.database.host"].Should().Be("localhost");
        result["root.debug"].Should().Be("true");
    }

    [Fact]
    public void Format_ReturnsXml()
    {
        _parser.Format.Should().Be("xml");
    }
}
