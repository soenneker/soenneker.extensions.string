﻿using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class IsNullOrEmptyBenchmark
{
    private string _value;

    [GlobalSetup]
    public void Setup()
    {
        _value = "3435";
    }

    [Benchmark(Baseline =true)]
    public bool IsNullOrEmptyBuiltIn()
    {
        return string.IsNullOrEmpty(_value);
    }

    [Benchmark]
    public bool IsNullOrEmpty()
    {
        return _value.IsNullOrEmpty();
    }
}