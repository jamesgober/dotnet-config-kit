using Xunit;
using dotnet_config_kit.Internal.Binding;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace dotnet_config_kit.Tests.EdgeCases;

/// <summary>
/// Edge case and boundary condition tests for production hardening.
/// </summary>
[SuppressMessage("Style", "CA1707:Identifiers should not contain underscores")]
public sealed class EdgeCaseTests
{
    [Fact]
    public void Bind_WithEmptyStringValue_BindsAsEmpty()
    {
        var config = new Dictionary<string, string> { { "value", "" } };
        var binder = new ReflectionConfigBinder<StringClass>();
        var result = binder.Bind(config);
        Assert.Empty(result.Value ?? "");
    }

    [Fact]
    public void Bind_WithNumericOverflow_ThrowsInvalidOperation()
    {
        var config = new Dictionary<string, string> { { "port", "999999999999999" } };
        var binder = new ReflectionConfigBinder<NumberClass>();
        Assert.Throws<InvalidOperationException>(() => binder.Bind(config));
    }

    [Fact]
    public void Bind_WithInvalidBoolean_ThrowsInvalidOperation()
    {
        var config = new Dictionary<string, string> { { "enabled", "maybe" } };
        var binder = new ReflectionConfigBinder<BoolClass>();
        Assert.Throws<InvalidOperationException>(() => binder.Bind(config));
    }

    [Fact]
    public void Bind_WithInvalidGuid_ThrowsInvalidOperation()
    {
        var config = new Dictionary<string, string> { { "id", "invalid" } };
        var binder = new ReflectionConfigBinder<GuidClass>();
        Assert.Throws<InvalidOperationException>(() => binder.Bind(config));
    }

    [Fact]
    public void Bind_WithEmptyConfig_UsesDefaults()
    {
        var config = new Dictionary<string, string>();
        var binder = new ReflectionConfigBinder<DefaultsClass>();
        var result = binder.Bind(config);
        Assert.Equal("default", result.Value);
        Assert.Equal(100, result.Count);
    }

    [Fact]
    public void Bind_CaseInsensitive_BindsCorrectly()
    {
        var config = new Dictionary<string, string>
        {
            { "MyProperty", "value1" },
            { "ANOTHERPROP", "value2" }
        };
        var binder = new ReflectionConfigBinder<CaseClass>();
        var result = binder.Bind(config);
        Assert.Equal("value1", result.MyProperty);
        Assert.Equal("value2", result.AnotherProp);
    }

    private sealed class StringClass
    {
        public string? Value { get; set; }
    }

    private sealed class NumberClass
    {
        public int Port { get; set; }
    }

    private sealed class BoolClass
    {
        public bool Enabled { get; set; }
    }

    private sealed class GuidClass
    {
        public Guid Id { get; set; }
    }

    private sealed class DefaultsClass
    {
        public string Value { get; set; } = "default";
        public int Count { get; set; } = 100;
    }

    private sealed class CaseClass
    {
        public string MyProperty { get; set; } = "";
        public string AnotherProp { get; set; } = "";
    }
}
