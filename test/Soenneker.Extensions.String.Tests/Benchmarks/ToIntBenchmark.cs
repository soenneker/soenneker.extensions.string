using System;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class ToIntBenchmark
{
    private string _value;

    [GlobalSetup]
    public void Setup()
    {
        _value = "3435343";
    }

    [Benchmark(Baseline =true)]
    public int ToIntBuiltIn()
    {
        return Convert.ToInt32(_value);
    }

    [Benchmark]
    public int ToInt()
    {
        return _value.ToInt();
    }
}