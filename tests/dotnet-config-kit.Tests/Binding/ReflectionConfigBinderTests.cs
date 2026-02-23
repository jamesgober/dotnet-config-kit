#pragma warning disable CA1707, CA2012
namespace dotnet_config_kit.Tests.Binding;

using dotnet_config_kit.Abstractions;
using dotnet_config_kit.Internal.Binding;
using FluentAssertions;
using Xunit;

public sealed class ReflectionConfigBinderTests
{
    public class SimpleConfig
    {
        public string? Name { get; set; }
        public int Port { get; set; }
        public bool Debug { get; set; }
    }

    public class ComplexConfig
    {
        public string? DatabaseHost { get; set; }
        public int DatabasePort { get; set; }
        public double Timeout { get; set; }
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EnumConfig
    {
        public LogLevel Level { get; set; }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    [Fact]
    public void Bind_SimpleConfig_BindsCorrectly()
    {
        var config = new Dictionary<string, string>
        {
            { "name", "testapp" },
            { "port", "8080" },
            { "debug", "true" }
        };

        var binder = new ReflectionConfigBinder<SimpleConfig>();
        var result = binder.Bind(config);

        result.Name.Should().Be("testapp");
        result.Port.Should().Be(8080);
        result.Debug.Should().BeTrue();
    }

    [Fact]
    public void Bind_MissingOptionalValues_UsesDefaults()
    {
        var config = new Dictionary<string, string>
        {
            { "name", "testapp" }
        };

        var binder = new ReflectionConfigBinder<SimpleConfig>();
        var result = binder.Bind(config);

        result.Name.Should().Be("testapp");
        result.Port.Should().Be(0);
        result.Debug.Should().BeFalse();
    }

    [Fact]
    public void Bind_InvalidIntValue_ThrowsInvalidOperationException()
    {
        var config = new Dictionary<string, string>
        {
            { "port", "invalid" }
        };

        var binder = new ReflectionConfigBinder<SimpleConfig>();

        var action = () => binder.Bind(config);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_CaseInsensitiveKeys()
    {
        var config = new Dictionary<string, string>
        {
            { "NAME", "testapp" },
            { "PORT", "8080" }
        };

        var binder = new ReflectionConfigBinder<SimpleConfig>();
        var result = binder.Bind(config);

        result.Name.Should().Be("testapp");
        result.Port.Should().Be(8080);
    }

    [Fact]
    public void Bind_ComplexTypes_ConvertsCorrectly()
    {
        var guid = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var config = new Dictionary<string, string>
        {
            { "databasehost", "localhost" },
            { "databaseport", "5432" },
            { "timeout", "30.5" },
            { "id", guid.ToString() },
            { "createdat", now.ToString("O") }
        };

        var binder = new ReflectionConfigBinder<ComplexConfig>();
        var result = binder.Bind(config);

        result.DatabaseHost.Should().Be("localhost");
        result.DatabasePort.Should().Be(5432);
        result.Timeout.Should().Be(30.5);
        result.Id.Should().Be(guid);
    }

    [Fact]
    public void Bind_EnumValue_ParsesCorrectly()
    {
        var config = new Dictionary<string, string>
        {
            { "level", "Warning" }
        };

        var binder = new ReflectionConfigBinder<EnumConfig>();
        var result = binder.Bind(config);

        result.Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void Bind_EnumValueCaseInsensitive()
    {
        var config = new Dictionary<string, string>
        {
            { "level", "debug" }
        };

        var binder = new ReflectionConfigBinder<EnumConfig>();
        var result = binder.Bind(config);

        result.Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void Bind_NullConfiguration_ThrowsArgumentNullException()
    {
        var binder = new ReflectionConfigBinder<SimpleConfig>();

        var action = () => binder.Bind(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task BindAsync_ReturnsValidResult()
    {
        var config = new Dictionary<string, string>
        {
            { "name", "testapp" }
        };

        var binder = new ReflectionConfigBinder<SimpleConfig>();
        var result = await binder.BindAsync(config, CancellationToken.None);

        result.Name.Should().Be("testapp");
    }

    [Fact]
    public void GetValidationErrors_ValidConfig_ReturnsEmpty()
    {
        var config = new Dictionary<string, string>
        {
            { "name", "testapp" },
            { "port", "8080" }
        };

        var binder = new ReflectionConfigBinder<SimpleConfig>();
        var errors = binder.GetValidationErrors(config);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void GetValidationErrors_InvalidValue_ReturnsError()
    {
        var config = new Dictionary<string, string>
        {
            { "port", "invalid" }
        };

        var binder = new ReflectionConfigBinder<SimpleConfig>();
        var errors = binder.GetValidationErrors(config);

        errors.Should().NotBeEmpty();
        errors[0].Path.Should().Be("port");
        errors[0].AttemptedValue.Should().Be("invalid");
    }

    [Fact]
    public void GetValidationErrors_NullConfiguration_ThrowsArgumentNullException()
    {
        var binder = new ReflectionConfigBinder<SimpleConfig>();

        var action = () => binder.GetValidationErrors(null!);
        action.Should().Throw<ArgumentNullException>();
    }
}
