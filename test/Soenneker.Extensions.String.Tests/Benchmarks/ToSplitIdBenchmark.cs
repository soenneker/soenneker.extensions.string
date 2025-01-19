using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class ToSplitIdBenchmark
{
    private string _value = null!;

    [GlobalSetup]
    public void Setup()
    {
        _value = "arst:blarsta";
    }

    [Benchmark(Baseline =true)]
    public (string, string) ToSplitIdBuiltIn()
    {
        return _value.ToSplitId();
    }

}
