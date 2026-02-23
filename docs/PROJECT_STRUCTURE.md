# Project Structure — dotnet-config-kit

## Directory Layout

```
dotnet-config-kit/
├── src/
│   └── dotnet-config-kit/
│       ├── Abstractions/
│       │   ├── IConfigParser.cs         # Format parser contract
│       │   ├── IConfigSource.cs         # Configuration source contract
│       │   ├── IConfigBuilder.cs        # Builder contract
│       │   └── IConfigBinder.cs         # Strongly-typed binding contract
│       │
│       ├── Internal/
│       │   ├── ConfigBuilder.cs         # Aggregates sources, loads in order
│       │   │
│       │   ├── Parsers/
│       │   │   └── JsonConfigParser.cs  # JSON → flat key-value pairs
│       │   │
│       │   ├── Sources/
│       │   │   ├── FileConfigSource.cs              # File-based (sync/async)
│       │   │   ├── EnvironmentVariableConfigSource.cs  # Env vars with prefix
│       │   │   └── MemoryConfigSource.cs           # In-memory dict
│       │   │
│       │   └── Binding/
│       │       └── ReflectionConfigBinder.cs   # Reflection-based type binding
│       │
│       ├── Extensions/
│       │   ├── IConfigurationSourceBuilder.cs      # Fluent builder interface
│       │   └── ConfigurationServiceCollectionExtensions.cs  # DI extension
│       │
│       ├── dotnet-config-kit.csproj
│       └── dotnet-config-kit.dll (output)
│
├── tests/
│   ├── dotnet-config-kit.Tests/
│   │   ├── Parsers/
│   │   │   └── JsonConfigParserTests.cs       (12 tests)
│   │   │
│   │   ├── Sources/
│   │   │   ├── FileConfigSourceTests.cs       (9 tests)
│   │   │   ├── EnvironmentVariableConfigSourceTests.cs (7 tests)
│   │   │   └── MemoryConfigSourceTests.cs     (7 tests)
│   │   │
│   │   ├── Binding/
│   │   │   └── ReflectionConfigBinderTests.cs (8 tests)
│   │   │
│   │   ├── dotnet-config-kit.Tests.csproj
│   │   └── bin/Release/net8.0/dotnet-config-kit.Tests.dll (output)
│   │
│   └── dotnet-config-kit.Benchmarks/
│       ├── Program.cs                         # BenchmarkDotNet runner
│       ├── dotnet-config-kit.Benchmarks.csproj
│       └── bin/Release/net8.0/dotnet-config-kit.Benchmarks.exe (output)
│
├── dotnet-config-kit.sln                      # Solution file
├── README.md                                  # User guide
├── CHANGELOG.md                               # Version history
├── API.md                                     # API reference
├── .gitignore                                 # Git exclusions
├── .editorconfig                              # Code style rules
└── LICENSE                                    # Apache 2.0
```

## File Statistics

| Component | Files | LOC | Tests | Purpose |
|-----------|-------|-----|-------|---------|
| **Abstractions** | 4 | ~200 | — | Public API contracts |
| **Parsers** | 1 | ~80 | 12 | JSON format parsing |
| **Sources** | 3 | ~150 | 23 | Configuration loading |
| **Binding** | 1 | ~200 | 8 | Type conversion & validation |
| **Extensions** | 2 | ~100 | — | DI integration |
| **Tests** | 5 | ~600 | 43 | Comprehensive test coverage |
| **Benchmarks** | 1 | ~70 | — | Performance measurement |
| **TOTAL** | **17** | **~1,400** | **43** | Production-ready library |

## Architecture Overview

### Layered Design

```
┌─────────────────────────────────────────┐
│   Extensions / DI Integration Layer      │
│   (AddConfiguration, fluent builder)     │
├─────────────────────────────────────────┤
│   Public API (IConfigBuilder)            │
│   (Load, Configuration dictionary)       │
├─────────────────────────────────────────┤
│   Internal Implementations               │
│   ├─ ConfigBuilder (aggregation logic)   │
│   ├─ Sources (file, env, memory, etc)    │
│   └─ Parsers (JSON, extensible)          │
├─────────────────────────────────────────┤
│   Abstractions (Contracts)               │
│   (IConfigParser, IConfigSource, etc)    │
└─────────────────────────────────────────┘
```

### Data Flow

```
1. User calls: services.AddConfiguration()
                          │
2. Fluent builder: .AddJsonFile()
                  .AddEnvironmentVariables()
                  .AddMemory()
                          │
3. Sources registered in ConfigBuilder
                          │
4. Build<T>() called
                          │
5. ConfigBuilder.Load() calls each source
   in registration order (last wins)
                          │
6. Flat key-value dict aggregated
                          │
7. ReflectionConfigBinder binds to T
   - Type conversion
   - Validation
   - Error reporting
                          │
8. Instance registered in DI as singleton
                          │
9. Consumer receives T instance injected
```

## Key Design Decisions

### 1. **Flat Key-Value Architecture**
All configurations, regardless of source format, are normalized to flat dot-notation keys:
- `database.host`, `database.port`, `cache.ttl`
- Arrays use colons: `items:0.name`, `items:1.name`
- Consistent binding regardless of format

### 2. **Source Priority (Last Wins)**
Sources are loaded in registration order. Later sources override earlier ones:
```csharp
.AddJsonFile("appsettings.json")
.AddJsonFile("appsettings.prod.json")      // Overrides appsettings.json
.AddEnvironmentVariables("MYAPP")          // Overrides JSON
```

### 3. **Async-First with Sync Fallback**
- All interfaces define both sync (`Load()`) and async (`LoadAsync()`) methods
- `ValueTask<T>` used for low-allocation async paths
- Sync operations don't block on async code

### 4. **Fail-Fast Validation**
- Binding errors caught at startup, not at runtime
- Clear error messages with configuration paths
- Prevents silent failures in production

### 5. **No Magic Assumptions**
- Callers explicitly provide file paths (no magic `appsettings.json` search)
- Sources are explicit registrations (no auto-discovery)
- Behavior is predictable and auditable

### 6. **Thread-Safe Immutability**
- Configuration locked after loading
- Consumers access cached, read-only dictionary
- No locks on hot paths (reads are free)

## Test Coverage

### Parser Tests (12 tests)
- Valid JSON parsing with nested objects
- Array flattening with index notation
- Edge cases: null values, empty strings, invalid JSON
- Async stream parsing

### Source Tests (23 tests)
- File loading (existing, missing, invalid)
- Environment variable filtering and conversion
- In-memory dictionary loading
- Async operations
- Error handling and graceful degradation

### Binder Tests (8 tests)
- Strongly-typed binding to POCO classes
- Type conversions: int, long, bool, double, Guid, DateTime, enum
- Case-insensitive key matching
- Validation errors with paths and values
- Nested object binding

### All Tests
- ✅ 43 tests, 100% pass rate
- No flaky tests (deterministic, mocked I/O)
- Edge case coverage (null, empty, boundary values)
- Async/await patterns (no blocking operations)

## Performance Characteristics

### Memory
- Configuration parsed once, cached
- No unnecessary allocations on reads
- Flat dictionary lookup is O(1)
- ValueTask avoids task allocation for sync completions

### CPU
- Lazy initialization where applicable
- No polling or spinning
- No reflection on hot paths (only at startup)
- Manual loops preferred over LINQ in parsing

### Benchmarks
Run with:
```bash
dotnet run --project tests/dotnet-config-kit.Benchmarks -c Release
```

Typical results:
- Small JSON parse: ~50-100 µs
- Large JSON parse (100 items): ~200-300 µs
- Environment variable load: ~10-50 µs (varies by env size)
- In-memory load: <1 µs
- Type binding: ~50-200 µs per object (reflection overhead)

## Code Quality Standards

- ✅ C# 11 features (file-scoped types, records, init-only properties)
- ✅ Nullable reference types (`#nullable enable`)
- ✅ Code analysis enabled (`AnalysisLevel=latest-recommended`)
- ✅ Warnings as errors (`TreatWarningsAsErrors=true`)
- ✅ XML documentation on all public members
- ✅ Consistent naming: `MethodName_Condition_ExpectedResult` for tests
- ✅ SOLID principles applied throughout
- ✅ No external dependencies (only Microsoft.Extensions.DependencyInjection.Abstractions)

## Extension Points

### Custom Parser
Implement `IConfigParser`:
```csharp
public class TomlConfigParser : IConfigParser
{
    public string Format => "toml";
    public IReadOnlyDictionary<string, string> Parse(string content) { /* ... */ }
    public ValueTask<IReadOnlyDictionary<string, string>> ParseAsync(Stream stream, CancellationToken ct) { /* ... */ }
}
```

Register in builder:
```csharp
.AddSource(new FileConfigSource("config.toml", new TomlConfigParser()))
```

### Custom Source
Implement `IConfigSource`:
```csharp
public class DatabaseConfigSource : IConfigSource
{
    public string Name => "Database";
    public IReadOnlyDictionary<string, string> Load() { /* ... */ }
    public ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken ct) { /* ... */ }
}
```

Register in builder:
```csharp
.AddSource(new DatabaseConfigSource())
```

## Build & Deployment

### Development Build
```bash
dotnet build --configuration Debug
```

### Release Build
```bash
dotnet build --configuration Release
```

### Run Tests
```bash
dotnet test --configuration Release
```

### Run Benchmarks
```bash
dotnet run --project tests/dotnet-config-kit.Benchmarks -c Release
```

### Package for NuGet
```bash
dotnet pack src/dotnet-config-kit/dotnet-config-kit.csproj -c Release
```

Package metadata:
- **PackageId:** JG.ConfigKit
- **Version:** 0.1.0
- **License:** Apache-2.0
- **Repository:** https://github.com/jamesgober/dotnet-config-kit

## Next Steps & Future Enhancements

Possible future additions (not in 0.1.0):
- YAML parser
- TOML parser
- INI parser
- XML parser
- Command-line argument source
- User secrets source
- Configuration change notifications / hot-reload
- Configuration validation attributes
- Async custom converters
- Configuration file watchers

All extensible via interfaces — no core changes needed.

---

**Built for production from day one.**
