namespace dotnet_config_kit.Internal.Sources;

using dotnet_config_kit.Abstractions;
using System.Net.Http;

/// <summary>
/// Loads configuration from a remote HTTP endpoint.
/// Supports optional polling for automatic updates.
/// </summary>
public sealed class HttpConfigSource : IConfigSource, IDisposable
{
    private readonly Uri _endpoint;
    private readonly IConfigParser _parser;
    private readonly HttpClient _httpClient;
    private readonly int? _pollIntervalSeconds;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task? _pollingTask;

    /// <inheritdoc />
    public string Name => $"HTTP: {_endpoint}";

    /// <summary>
    /// Creates a new HTTP configuration source.
    /// </summary>
    /// <param name="endpoint">The HTTP endpoint URL. Must not be null and must be HTTP or HTTPS.</param>
    /// <param name="parser">The parser for the response format. Must not be null.</param>
    /// <param name="pollIntervalSeconds">Optional polling interval in seconds. If set, configuration is reloaded periodically.</param>
    /// <param name="httpClient">Optional custom HttpClient. If null, a new one is created.</param>
    /// <exception cref="ArgumentNullException">Thrown when endpoint or parser is null.</exception>
    /// <exception cref="ArgumentException">Thrown when endpoint is not HTTP/HTTPS or pollInterval is less than or equal to 0.</exception>
    public HttpConfigSource(Uri endpoint, IConfigParser parser, int? pollIntervalSeconds = null, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(endpoint, nameof(endpoint));
        ArgumentNullException.ThrowIfNull(parser, nameof(parser));

        if (endpoint.Scheme != "http" && endpoint.Scheme != "https")
        {
            throw new ArgumentException("Endpoint must be HTTP or HTTPS", nameof(endpoint));
        }

        if (pollIntervalSeconds.HasValue && pollIntervalSeconds <= 0)
        {
            throw new ArgumentException("Poll interval must be greater than 0", nameof(pollIntervalSeconds));
        }

        _endpoint = endpoint;
        _parser = parser;
        _pollIntervalSeconds = pollIntervalSeconds;
        _httpClient = httpClient ?? new HttpClient();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load()
    {
        try
        {
            var response = _httpClient.GetAsync(_endpoint).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return _parser.Parse(content);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from {_endpoint}: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading configuration from {_endpoint}: {ex.Message}", ex);
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyDictionary<string, string>> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);
            var response = await _httpClient.GetAsync(_endpoint, cts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
            return _parser.Parse(content);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to load configuration from {_endpoint}: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading configuration from {_endpoint}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Starts polling for configuration updates if polling is enabled.
    /// </summary>
    public void StartPolling(Func<IReadOnlyDictionary<string, string>, CancellationToken, ValueTask> onChangeCallback)
    {
        ArgumentNullException.ThrowIfNull(onChangeCallback, nameof(onChangeCallback));

        if (!_pollIntervalSeconds.HasValue)
        {
            return;
        }

        _pollingTask = PollAsync(onChangeCallback, _cancellationTokenSource.Token);
    }

    private async Task PollAsync(Func<IReadOnlyDictionary<string, string>, CancellationToken, ValueTask> onChangeCallback, CancellationToken cancellationToken)
    {
        var pollInterval = TimeSpan.FromSeconds(_pollIntervalSeconds!.Value);
        IReadOnlyDictionary<string, string>? lastConfig = null;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(pollInterval, cancellationToken).ConfigureAwait(false);

                try
                {
                    var currentConfig = await LoadAsync(cancellationToken).ConfigureAwait(false);

                    // Check if config changed
                    if (lastConfig == null || !ConfigEquals(lastConfig, currentConfig))
                    {
                        lastConfig = currentConfig;
                        await onChangeCallback(currentConfig, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error polling configuration from {_endpoint}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
    }

    private static bool ConfigEquals(IReadOnlyDictionary<string, string> a, IReadOnlyDictionary<string, string> b)
    {
        if (a.Count != b.Count)
            return false;

        foreach (var kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out var bValue) || kvp.Value != bValue)
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _pollingTask?.Wait(TimeSpan.FromSeconds(5));
        _cancellationTokenSource?.Dispose();
        _httpClient?.Dispose();
    }
}
