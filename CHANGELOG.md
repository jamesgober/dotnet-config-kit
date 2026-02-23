# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/jamesgober/dotnet-config-kit/compare/0.1.0...HEAD
[0.1.0]: https://github.com/jamesgober/dotnet-config-kit/releases/tag/0.1.0
