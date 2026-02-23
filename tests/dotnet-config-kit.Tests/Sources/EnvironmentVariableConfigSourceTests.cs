#pragma warning disable CA1707
namespace dotnet_config_kit.Tests.Sources;

using dotnet_config_kit.Internal.Sources;
using FluentAssertions;
using Xunit;
using System.IO;

public sealed class EnvironmentVariableConfigSourceTests
{
    [Fact]
    public void Load_WithoutPrefix_ReturnsAllEnvironmentVariables()
    {
        var source = new EnvironmentVariableConfigSource();

        var result = source.Load();

        result.Should().NotBeEmpty();
        result.Keys.Should().AllSatisfy(k => k.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void Load_WithPrefix_FiltersVariables()
    {
        Environment.SetEnvironmentVariable("TESTAPP_DATABASE_HOST", "localhost");
        Environment.SetEnvironmentVariable("TESTAPP_DATABASE_PORT", "5432");
        Environment.SetEnvironmentVariable("OTHER_VAR", "ignored");

        var source = new EnvironmentVariableConfigSource("TESTAPP");
        var result = source.Load();

        Environment.SetEnvironmentVariable("TESTAPP_DATABASE_HOST", null);
        Environment.SetEnvironmentVariable("TESTAPP_DATABASE_PORT", null);
        Environment.SetEnvironmentVariable("OTHER_VAR", null);

        result.Should().ContainKey("database.host");
        result.Should().ContainKey("database.port");
        result.Should().NotContainKey("other.var");
    }

    [Fact]
    public void Load_ConvertDoubleUnderscoreToDot()
    {
        Environment.SetEnvironmentVariable("TEST_SUBSECTION__SETTING", "value");

        var source = new EnvironmentVariableConfigSource("TEST");
        var result = source.Load();

        Environment.SetEnvironmentVariable("TEST_SUBSECTION__SETTING", null);

        result.Should().ContainKey("subsection.setting");
    }

    [Fact]
    public async Task LoadAsync_ReturnsResult()
    {
        var source = new EnvironmentVariableConfigSource();

        var result = await source.LoadAsync(CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public void Name_WithoutPrefix_ReturnsGenericName()
    {
        var source = new EnvironmentVariableConfigSource();

        source.Name.Should().Be("Environment Variables");
    }

    [Fact]
    public void Name_WithPrefix_IncludesPrefix()
    {
        var source = new EnvironmentVariableConfigSource("MYAPP");

        source.Name.Should().Contain("MYAPP");
    }
}
