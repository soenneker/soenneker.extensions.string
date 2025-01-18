using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class ToUpperInvariantBenchmark
{
    private string _value;

    [GlobalSetup]
    public void Setup()
    {
        _value = "34352342arstA";
    }

    [Benchmark(Baseline =true)]
    public string ToUpperInvariantBuiltIn()
    {
        return _value.ToUpperInvariant();
    }

    [Benchmark]
    public string ToUpperInvariantFast()
    {
        return _value.ToUpperInvariantFast();
    }
}