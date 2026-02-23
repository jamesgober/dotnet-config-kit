#pragma warning disable CA1707
namespace dotnet_config_kit.Tests.Sources;

using dotnet_config_kit.Internal.Sources;
using dotnet_config_kit.Internal.Parsers;
using FluentAssertions;
using Xunit;
using System.IO;

public sealed class FileConfigSourceTests : IDisposable
{
    private readonly string _testDir = Path.Combine(Path.GetTempPath(), $"configtest_{Guid.NewGuid():N}");

    public FileConfigSourceTests()
    {
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void Load_FileExists_ReturnsConfiguration()
    {
        var filePath = Path.Combine(_testDir, "config.json");
        File.WriteAllText(filePath, """{"key": "value"}""");

        var source = new FileConfigSource(filePath, new JsonConfigParser());
        var result = source.Load();

        result.Should().ContainKey("key");
        result["key"].Should().Be("value");
    }

    [Fact]
    public void Load_FileDoesNotExist_ReturnsEmptyDictionary()
    {
        var filePath = Path.Combine(_testDir, "nonexistent.json");

        var source = new FileConfigSource(filePath, new JsonConfigParser());
        var result = source.Load();

        result.Should().BeEmpty();
    }

    [Fact]
    public void Load_InvalidJson_ThrowsInvalidOperationException()
    {
        var filePath = Path.Combine(_testDir, "bad.json");
        File.WriteAllText(filePath, "{invalid}");

        var source = new FileConfigSource(filePath, new JsonConfigParser());

        var action = () => source.Load();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_NullFilePath_ThrowsArgumentNullException()
    {
        var action = () => new FileConfigSource(null!, new JsonConfigParser());

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyFilePath_ThrowsArgumentException()
    {
        var action = () => new FileConfigSource("", new JsonConfigParser());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NullParser_ThrowsArgumentNullException()
    {
        var action = () => new FileConfigSource("path", null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task LoadAsync_FileExists_ReturnsConfiguration()
    {
        var filePath = Path.Combine(_testDir, "config.json");
        File.WriteAllText(filePath, """{"key": "value"}""");

        var source = new FileConfigSource(filePath, new JsonConfigParser());
        var result = await source.LoadAsync(CancellationToken.None);

        result.Should().ContainKey("key");
        result["key"].Should().Be("value");
    }

    [Fact]
    public void Name_IncludesFilePath()
    {
        var filePath = Path.Combine(_testDir, "config.json");
        var source = new FileConfigSource(filePath, new JsonConfigParser());

        source.Name.Should().Contain(filePath);
    }
}
