using System;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class ToBytesFromBase64Benchmark
{
    private string _value;

    [GlobalSetup]
    public void Setup()
    {
        _value = "3435343";
    }

    [Benchmark(Baseline =true)]
    public byte[] ToBytesFromBase64BuiltIn()
    {
        return Convert.FromBase64String(_value);
    }

    [Benchmark]
    public byte[] ToBytesFromBase64()
    {
        return _value.ToBytesFromBase64();
    }
}
