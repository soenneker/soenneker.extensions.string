using BenchmarkDotNet.Attributes;
using System;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class ToBoolBenchmark
{
    private string _value = null!;

    [GlobalSetup]
    public void Setup()
    {
        _value = "true";
    }

    [Benchmark(Baseline = true)]
    public bool ToBoolBuiltIn()
    {
        return Convert.ToBoolean(_value);
    }
}
