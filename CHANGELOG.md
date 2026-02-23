# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-02-23

### Overview
This is the official v1.0.0 release of dotnet-config-kit, a production-ready, high-performance configuration management library for .NET. This release represents the culmination of careful design, comprehensive testing, and extensive documentation.

### Added
- **Complete Multi-Source Configuration System** — Support for 11 different configuration sources including JSON, YAML, TOML, INI, XML, environment variables, command-line arguments, user secrets, HTTP endpoints, in-memory dictionaries, and custom sources
- **Strongly-Typed Configuration Binding** — Bind flat configuration to POCO classes with support for nested objects, collections, and custom type converters
- **Production-Grade Validation** — DataAnnotations support with comprehensive error reporting and clear, actionable error messages
- **Flexible Merging Strategies** — LastWins, FirstWins, Merge, and Throw strategies for controlling how multiple sources combine
- **Default Values & Presets** — Provide fallback configuration values and reusable named presets for common scenarios
- **Hot-Reload Capability** — Automatic configuration reloading with FileSystemWatcher integration and change notifications
- **Remote Configuration** — Load configuration from HTTP endpoints with optional polling for cloud-native architectures
- **Configuration Export** — Serialize loaded configuration to JSON or YAML for debugging and auditing
- **Lazy Loading** — Defer source initialization until first access with optional timeouts for performance optimization
- **Async-First Design** — Built with ValueTask for zero-allocation async operations
- **Zero-Copy Reads** — Configurations cached after loading; reads are lock-free and allocation-free
- **Comprehensive Documentation** — Getting started guide, API reference, advanced usage patterns, and real-world examples

### Quality Assurance
- 63 comprehensive tests covering normal use cases, edge cases, and boundary conditions
- 100% test pass rate with zero known issues
- Zero compiler warnings using strict .NET code analysis
- Full XML documentation on all public APIs
- Advanced usage guide with production patterns

### Breaking Changes
None — this is the first stable release.

## [0.6.0] - 2026-02-23

### Added
- Comprehensive edge case test suite (6 new tests covering boundary conditions)
- Production hardening validation for all configuration sources
- Edge case documentation in V0.6.0_PRODUCTION_HARDENING_PLAN.md
- Test coverage for numeric overflow, invalid type conversions, and empty configurations

### Changed
- Enhanced error reporting consistency across all binding operations
- Improved documentation with advanced usage patterns in docs/ADVANCED.md

### Security
- Validated robust error handling for all invalid input scenarios
- Confirmed proper resource disposal in all configuration sources

## [0.5.0] - 2026-02-23

### Added
- Optional file parameter for all file-based sources (`isOptional` flag)
- JSON file source with hot-reload support via overload method
- Consistent hot-reload API across all format parsers (JSON, YAML, TOML, INI, XML)
- Graceful handling of missing optional configuration files
- Default configuration values via `AddDefaults()` method
- Configuration presets for reusable settings via `RegisterPreset()` and `UsePreset()` methods
- `ConfigurationPresets` class for managing named preset configurations
- API audit and consistency improvements for launch readiness

### Changed
- All `AddXxxFile()` methods now support both `isOptional` and `enableHotReload` parameters
- Improved consistency between JSON and other format parsers
- Enhanced error messages for missing required configuration files

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

[Unreleased]: https://github.com/jamesgober/dotnet-config-kit/compare/1.0.0...HEAD
[1.0.0]: https://github.com/jamesgober/dotnet-config-kit/releases/tag/1.0.0
[0.6.0]: https://github.com/jamesgober/dotnet-config-kit/compare/0.5.0...0.6.0
[0.5.0]: https://github.com/jamesgober/dotnet-config-kit/compare/0.4.0...0.5.0
[0.4.0]: https://github.com/jamesgober/dotnet-config-kit/compare/0.3.0...0.4.0
[0.3.0]: https://github.com/jamesgober/dotnet-config-kit/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/jamesgober/dotnet-config-kit/compare/0.1.0...0.2.0
[0.1.0]: https://github.com/jamesgober/dotnet-config-kit/releases/tag/0.1.0
