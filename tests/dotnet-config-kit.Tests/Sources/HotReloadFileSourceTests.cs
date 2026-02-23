#pragma warning disable CA1707
namespace dotnet_config_kit.Tests.Sources;

using dotnet_config_kit.Internal.Sources;
using dotnet_config_kit.Internal.Parsers;
using FluentAssertions;
using Xunit;
using System.IO;

public sealed class HotReloadFileSourceTests : IDisposable
{
    private readonly string _testDir = Path.Combine(Path.GetTempPath(), $"hotreload_{Guid.NewGuid():N}");

    public HotReloadFileSourceTests()
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
    public void Load_InitialLoad_ReturnsConfiguration()
    {
        var filePath = Path.Combine(_testDir, "config.json");
        File.WriteAllText(filePath, """{"key": "value1"}""");

        var source = new FileConfigSource(filePath, new JsonConfigParser());
        var hotReload = new HotReloadFileSource(source, enableWatcher: false);
        
        var config = hotReload.Load();
        
        config.Should().ContainKey("key");
        config["key"].Should().Be("value1");
    }

    [Fact]
    public async Task OnChange_FileModified_CallsCallback()
    {
        var filePath = Path.Combine(_testDir, "config.json");
        File.WriteAllText(filePath, """{"key": "value1"}""");

        var source = new FileConfigSource(filePath, new JsonConfigParser());
        var hotReload = new HotReloadFileSource(source, enableWatcher: true);

        var callCount = 0;
        hotReload.OnChange(_ => { callCount++; });

        // Modify file
        await Task.Delay(200);
        File.WriteAllText(filePath, """{"key": "value2"}""");

        // Wait for file watcher
        await Task.Delay(500);

        // Callback should have been called
        callCount.Should().BeGreaterThan(0);
        
        hotReload.Dispose();
    }

    [Fact]
    public void Name_IncludesHotReloadPrefix()
    {
        var filePath = Path.Combine(_testDir, "config.json");
        File.WriteAllText(filePath, """{}""");

        var source = new FileConfigSource(filePath, new JsonConfigParser());
        var hotReload = new HotReloadFileSource(source, enableWatcher: false);

        hotReload.Name.Should().Contain("HotReload");
        
        hotReload.Dispose();
    }

    [Fact]
    public void Dispose_AllowsCleanup()
    {
        var filePath = Path.Combine(_testDir, "config.json");
        File.WriteAllText(filePath, """{}""");

        var source = new FileConfigSource(filePath, new JsonConfigParser());
        var hotReload = new HotReloadFileSource(source, enableWatcher: true);

        // Dispose should not throw
        hotReload.Dispose();

        // Accessing after dispose should throw
        var action = () => hotReload.Load();
        action.Should().Throw<ObjectDisposedException>();
    }
}
