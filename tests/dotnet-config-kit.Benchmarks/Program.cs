#pragma warning disable CA1050
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using dotnet_config_kit.Internal.Parsers;

var summary = BenchmarkRunner.Run<JsonParserBenchmark>();
Console.WriteLine(summary);

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class JsonParserBenchmark
{
    private readonly JsonConfigParser _parser = new();
    private string _smallJson = "";
    private string _largeJson = "";

    [GlobalSetup]
    public void Setup()
    {
        _smallJson = """
            {
              "database": {
                "host": "localhost",
                "port": "5432",
                "username": "admin",
                "password": "secret"
              },
              "cache": {
                "enabled": true,
                "ttl": 3600
              }
            }
            """;

        var items = string.Join(",", Enumerable.Range(0, 100).Select(i =>
            $$$"""{ "id": {{{i}}}, "name": "item{{{i}}}", "value": {{{i * 10}}} }}"""));

        _largeJson = $$$"""
            {
              "metadata": {"version": "1.0", "timestamp": "2024-01-01T00:00:00Z"},
              "items": [{{{items}}}]
            }
            """;
    }

    [Benchmark]
    public void ParseSmallJson()
    {
        _parser.Parse(_smallJson);
    }

    [Benchmark]
    public void ParseLargeJson()
    {
        _parser.Parse(_largeJson);
    }

    [Benchmark]
    public async System.Threading.Tasks.Task ParseSmallJsonAsync()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_smallJson));
        await _parser.ParseAsync(stream);
    }

    [Benchmark]
    public async System.Threading.Tasks.Task ParseLargeJsonAsync()
    {
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_largeJson));
        await _parser.ParseAsync(stream);
    }
}
