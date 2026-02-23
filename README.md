<div align="center">
    <img width="120px" height="auto" src="https://raw.githubusercontent.com/jamesgober/jamesgober/main/media/icons/hexagon-3.svg" alt="Triple Hexagon">
    <h1>
        <strong>dotnet-config-kit</strong>
        <sup><br><sub>CONFIGURATION MANAGEMENT</sub></sup>
    </h1>
    <div>
        <a href="https://www.nuget.org/packages/dotnet-config-kit"><img alt="NuGet" src="https://img.shields.io/nuget/v/dotnet-config-kit"></a>
        <span>&nbsp;</span>
        <a href="https://www.nuget.org/packages/dotnet-config-kit"><img alt="NuGet Downloads" src="https://img.shields.io/nuget/dt/dotnet-config-kit?color=%230099ff"></a>
        <span>&nbsp;</span>
        <a href="./LICENSE" title="License"><img alt="License" src="https://img.shields.io/badge/license-Apache--2.0-blue.svg"></a>
        <span>&nbsp;</span>
        <a href="https://github.com/jamesgober/dotnet-config-kit/actions"><img alt="GitHub CI" src="https://github.com/jamesgober/dotnet-config-kit/actions/workflows/ci.yml/badge.svg"></a>
    </div>
</div>
<br>
<p>
    A lightweight, high-performance configuration library for .NET applications. Loads from multiple sources (JSON files, environment variables, in-memory dictionaries, user secrets), binds to strongly-typed options classes, validates on startup, and optionally hot-reloads on file changes — all with minimal overhead.
</p>

## Features

- **Multi-Source Loading** — JSON files, environment variables, command-line args, in-memory providers, and user secrets with layered override priority
- **Strongly-Typed Binding** — Bind configuration sections directly to POCO classes with full nested object support
- **Validation** — Data annotation validation on startup with clear error reporting; fail fast, not at runtime
- **Hot-Reload** — Optional file-watcher that pushes updated config without restart
- **Environment Awareness** — Automatic `appsettings.{Environment}.json` layering
- **Minimal API** — Single extension method: `services.AddAppConfig<T>()`
- **Zero Allocation Reads** — Cached options instances; reads are lock-free after initial bind

<br>

## Installation

```bash
dotnet add package dotnet-config-kit
```

<br>

## Quick Start

```csharp
// Register configuration in Program.cs
builder.Services.AddAppConfig<AppSettings>();

// Inject anywhere via DI
public class MyService(IOptions<AppSettings> options)
{
    private readonly AppSettings _config = options.Value;
}
```

<br>

## Documentation

- **[API Reference](./docs/API.md)** — Full API documentation and examples

<br>

## Contributing

Contributions welcome. Please:
1. Ensure all tests pass before submitting
2. Follow existing code style and patterns
3. Update documentation as needed

<br>

## Testing

```bash
dotnet test
```

<br>
<hr>
<br>

<div id="license">
    <h2>⚖️ License</h2>
    <p>Licensed under the <b>Apache License</b>, version 2.0 (the <b>"License"</b>); you may not use this software, including, but not limited to the source code, media files, ideas, techniques, or any other associated property or concept belonging to, associated with, or otherwise packaged with this software except in compliance with the <b>License</b>.</p>
    <p>You may obtain a copy of the <b>License</b> at: <a href="http://www.apache.org/licenses/LICENSE-2.0" title="Apache-2.0 License" target="_blank">http://www.apache.org/licenses/LICENSE-2.0</a>.</p>
    <p>Unless required by applicable law or agreed to in writing, software distributed under the <b>License</b> is distributed on an "<b>AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND</b>, either express or implied.</p>
    <p>See the <a href="./LICENSE" title="Software License file">LICENSE</a> file included with this project for the specific language governing permissions and limitations under the <b>License</b>.</p>
    <br>
</div>

<div align="center">
    <h2></h2>
    <sup>COPYRIGHT <small>&copy;</small> 2025 <strong>JAMES GOBER.</strong></sup>
</div>
