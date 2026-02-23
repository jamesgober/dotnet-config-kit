#pragma warning disable CA1707
namespace dotnet_config_kit.Tests.Sources;

using dotnet_config_kit.Internal.Sources;
using FluentAssertions;
using Xunit;

public sealed class MemoryConfigSourceTests
{
    [Fact]
    public void Load_ReturnsProvidedData()
    {
        var data = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        var source = new MemoryConfigSource(data);
        var result = source.Load();

        result.Should().ContainKey("key1");
        result.Should().ContainKey("key2");
        result["key1"].Should().Be("value1");
        result["key2"].Should().Be("value2");
    }

    [Fact]
    public void Load_CaseInsensitive()
    {
        var data = new Dictionary<string, string>
        {
            { "Key1", "value1" }
        };

        var source = new MemoryConfigSource(data);
        var result = source.Load();

        result.Should().ContainKey("key1");
        result.Should().ContainKey("Key1");
    }

    [Fact]
    public void Constructor_NullData_ThrowsArgumentNullException()
    {
        var action = () => new MemoryConfigSource(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task LoadAsync_ReturnsData()
    {
        var data = new[] { new KeyValuePair<string, string>("key", "value") };

        var source = new MemoryConfigSource(data);
        var result = await source.LoadAsync(CancellationToken.None);

        result.Should().ContainKey("key");
        result["key"].Should().Be("value");
    }

    [Fact]
    public void Name_DefaultValue_ReturnsMemory()
    {
        var data = new Dictionary<string, string>();
        var source = new MemoryConfigSource(data);

        source.Name.Should().Be("Memory");
    }

    [Fact]
    public void Name_CustomValue_ReturnsProvidedName()
    {
        var data = new Dictionary<string, string>();
        var source = new MemoryConfigSource(data, "CustomSource");

        source.Name.Should().Be("CustomSource");
    }
}
