namespace dotnet_config_kit.Internal.Sources;

using dotnet_config_kit.Abstractions;

/// <summary>
/// Wraps a file-based config source with hot-reload capability.
/// Watches for file changes and notifies subscribers.
/// </summary>
public sealed class HotReloadFileSource : IConfigSource, IDisposable
{
    private readonly FileConfigSource _source;
    private readonly FileSystemWatcher? _watcher;
    private readonly object _lock = new();
    private readonly List<Func<IReadOnlyDictionary<string, string>, CancellationToken, ValueTask>> _subscribers = new();
    private volatile bool _disposed;
    private IReadOnlyDictionary<string, string> _currentConfig;

    /// <inheritdoc />
    public string Name => $"HotReload({_source.Name})";

    /// <summary>
    /// Creates a new hot-reload file source.
    /// </summary>
    /// <param name="source">The underlying file source. Must not be null.</param>
    /// <param name="enableWatcher">Whether to enable file watching. If false, behaves as normal source.</param>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    public HotReloadFileSource(FileConfigSource source, bool enableWatcher = true)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        _source = source;
        _currentConfig = _source.Load();

        if (!enableWatcher)
        {
            _watcher = null;
            return;
        }

        // Extract file path from source name (format: "File: path")
        var sourceName = _source.Name;
        var pathStart = sourceName.IndexOf(": ", StringComparison.Ordinal);
        if (pathStart < 0)
        {
            _watcher = null;
            return;
        }

        var filePath = sourceName.Substring(pathStart + 2);
        var directory = Path.GetDirectoryName(filePath) ?? ".";
        var fileName = Path.GetFileName(filePath);

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load()
    {
        ThrowIfDisposed();
        
        lock (_lock)
        {
            _currentConfig = _source.Load();
            return _currentConfig;
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            _currentConfig = _source.Load();
            return _currentConfig;
        }
    }

    /// <summary>
    /// Subscribes to configuration changes.
    /// </summary>
    /// <param name="callback">Called when configuration changes. Receives new config and cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
    public void OnChange(Func<IReadOnlyDictionary<string, string>, CancellationToken, ValueTask> callback)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));

        lock (_lock)
        {
            _subscribers.Add(callback);
        }
    }

    /// <summary>
    /// Subscribes to configuration changes (synchronous callback).
    /// </summary>
    /// <param name="callback">Called when configuration changes. Receives new config.</param>
    public void OnChange(Action<IReadOnlyDictionary<string, string>> callback)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(callback, nameof(callback));

        lock (_lock)
        {
            _subscribers.Add((config, _) =>
            {
                callback(config);
                return default;
            });
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            // Debounce: wait a bit for file writes to complete
            System.Threading.Thread.Sleep(100);

            IReadOnlyDictionary<string, string> newConfig;
            lock (_lock)
            {
                newConfig = _source.Load();
                _currentConfig = newConfig;
            }

            // Notify all subscribers
            _ = NotifySubscribersAsync(newConfig, CancellationToken.None);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reloading configuration: {ex.Message}");
        }
    }

    private async Task NotifySubscribersAsync(IReadOnlyDictionary<string, string> config, CancellationToken cancellationToken)
    {
        List<Func<IReadOnlyDictionary<string, string>, CancellationToken, ValueTask>> subscribers;
        lock (_lock)
        {
            subscribers = new List<Func<IReadOnlyDictionary<string, string>, CancellationToken, ValueTask>>(_subscribers);
        }

        foreach (var callback in subscribers)
        {
            try
            {
                await callback(config, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in configuration change callback: {ex.Message}");
            }
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(HotReloadFileSource));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _watcher?.Dispose();
    }
}
