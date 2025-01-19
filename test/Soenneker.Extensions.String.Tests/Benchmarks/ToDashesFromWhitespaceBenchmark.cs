using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class ToDashesFromWhitespaceBenchmark
{
    private string _value = null!;

    [GlobalSetup]
    public void Setup()
    {
        _value = "3435343 arst \n blargwf \n\r";
    }

    [Benchmark(Baseline = true)]
    public string ToDashesFromWhitespaceBuiltIn()
    {
        return _value.Replace(" ", "-");
    }

    [Benchmark]
    public string ToDashesFromWhitespace()
    {
        return _value.ToDashesFromWhitespace();
    }
}
