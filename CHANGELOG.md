# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.4.0] - 2026-02-23

### Added
- Remote HTTP configuration source with optional polling for automatic updates
- Configuration merging strategies: `LastWins`, `FirstWins`, `Merge`, `Throw`
- Configuration serialization to JSON and YAML formats for export and debugging
- Lazy configuration loading with optional timeout for deferred initialization
- `AddHttpSource()` extension method for HTTP endpoints with configurable polling
- `WithMergeStrategy()` method to control how multiple sources are combined
- `AddLazySource()` method to defer source loading until first access
- `ExportAsJson()` and `ExportAsYaml()` methods for configuration export
- `HttpConfigSource` class for loading configuration from remote endpoints
- `ConfigurationMerger` for flexible source combination strategies
- `ConfigurationSerializer` for exporting configuration to standard formats
- `LazyConfigSource` wrapper for lazy-loaded configuration sources

## [0.3.0] - 2026-02-23

### Added
- Command-line arguments source with support for `--key=value`, `--key value`, and `-k value` formats
- User secrets source with `UserSecretsIdAttribute` integration
- Custom type converters via `IConfigValueConverter<T>` interface for extensible type conversion
- DataAnnotations validation support with `[Required]`, `[Range]`, `[EmailAddress]`, etc.
- Configuration profiles for environment-specific settings (development, staging, production)
- `WithProfile()` and `WithAutoProfile()` extension methods for profile management
- Automatic profile detection from `ASPNETCORE_ENVIRONMENT`, `ENVIRONMENT`, or `DOTNET_ENVIRONMENT` variables
- `EnhancedReflectionConfigBinder<T>` with full validation attribute support
- `.RegisterConverter<T>()` method for custom type conversion

## [0.2.0] - 2026-02-23

### Added
- YAML format parser via YamlDotNet with full nested structure support
- TOML format parser via Tomlyn with table and array support
- INI format parser with section and key-value support
- XML format parser with element and attribute support
- Hot-reload capability for file-based configuration sources with FileSystemWatcher
- Configuration change notifications via callback subscribers
- Extension methods for all format parsers: `AddYamlFile()`, `AddTomlFile()`, `AddIniFile()`, `AddXmlFile()`
- Optional `enableHotReload` parameter on all file source methods
- `OnChange()` extension for subscribing to configuration changes
- Cross-platform file watching support

## [0.1.0] - 2026-01-15

### Added
- Core abstractions: `IConfigParser`, `IConfigSource`, `IConfigBinder<T>`, `IConfigBuilder`
- JSON format parser with full nested object and array support
- File-based configuration source with async load support
- Environment variable source with prefix filtering and underscore-to-dot conversion
- In-memory configuration source for testing and programmatic configuration
- Reflection-based configuration binder with type conversion for primitives, enums, GUID, and DateTime
- Configuration validation with error reporting including paths and attempted values
- Dependency injection integration via `AddConfiguration()` extension method
- Fluent builder API for registering sources and binding to options types
- Comprehensive unit tests for all core components
- Performance benchmarks using BenchmarkDotNet
- Full XML documentation on all public APIs

[Unreleased]: https://github.com/jamesgober/dotnet-config-kit/compare/0.4.0...HEAD
[0.4.0]: https://github.com/jamesgober/dotnet-config-kit/compare/0.3.0...0.4.0
[0.3.0]: https://github.com/jamesgober/dotnet-config-kit/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/jamesgober/dotnet-config-kit/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/jamesgober/dotnet-config-kit/releases/tag/0.1.0
