namespace dotnet_config_kit.Internal;

using dotnet_config_kit.Abstractions;

/// <summary>
/// Wrapper for lazy-loading configuration sources.
/// Defers loading until first access.
/// </summary>
public sealed class LazyConfigSource : IConfigSource
{
    private readonly IConfigSource _innerSource;
    private readonly int? _timeoutSeconds;
    private IReadOnlyDictionary<string, string>? _cachedConfig;
    private bool _loaded;
    private readonly object _lock = new();

    /// <inheritdoc />
    public string Name => $"Lazy({_innerSource.Name})";

    /// <summary>
    /// Creates a new lazy config source.
    /// </summary>
    /// <param name="innerSource">The underlying source to lazily load.</param>
    /// <param name="timeoutSeconds">Optional timeout for load operations in seconds.</param>
    public LazyConfigSource(IConfigSource innerSource, int? timeoutSeconds = null)
    {
        ArgumentNullException.ThrowIfNull(innerSource, nameof(innerSource));
        
        if (timeoutSeconds.HasValue && timeoutSeconds <= 0)
        {
            throw new ArgumentException("Timeout must be greater than 0", nameof(timeoutSeconds));
        }

        _innerSource = innerSource;
        _timeoutSeconds = timeoutSeconds;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load()
    {
        lock (_lock)
        {
            if (!_loaded)
            {
                if (_timeoutSeconds.HasValue)
                {
                    var task = Task.Run(() => _innerSource.Load());
                    if (!task.Wait(TimeSpan.FromSeconds(_timeoutSeconds.Value)))
                    {
                        throw new TimeoutException(
                            $"Configuration load from {_innerSource.Name} exceeded {_timeoutSeconds} seconds");
                    }
                    _cachedConfig = task.Result;
                }
                else
                {
                    _cachedConfig = _innerSource.Load();
                }

                _loaded = true;
            }

            return _cachedConfig!;
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_loaded)
            {
                return _cachedConfig!;
            }
        }

        try
        {
            IReadOnlyDictionary<string, string>? config;

            if (_timeoutSeconds.HasValue)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds.Value));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
                config = await _innerSource.LoadAsync(linkedCts.Token).ConfigureAwait(false);
            }
            else
            {
                config = await _innerSource.LoadAsync(cancellationToken).ConfigureAwait(false);
            }

            lock (_lock)
            {
                _cachedConfig = config;
                _loaded = true;
            }

            return config;
        }
        catch (OperationCanceledException) when (_timeoutSeconds.HasValue)
        {
            throw new TimeoutException(
                $"Configuration load from {_innerSource.Name} exceeded {_timeoutSeconds} seconds");
        }
    }
}
