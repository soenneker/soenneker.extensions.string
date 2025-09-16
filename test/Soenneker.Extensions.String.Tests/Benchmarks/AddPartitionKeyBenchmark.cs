using System;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class AddPartitionKeyBenchmark
{
    private string _documentId = null!;
    private string _partitionKey = null!;

    [GlobalSetup]
    public void Setup()
    {
        _documentId = Guid.NewGuid().ToString();
        _partitionKey = Guid.NewGuid().ToString();
    }

    [Benchmark(Baseline = true)]
    public string StringConcat()
    {
        return _partitionKey + ':' + _documentId;
    }

    [Benchmark]
    public string StringInterpolation()
    {
        return $"{_partitionKey}:{_documentId}";
    }

    [Benchmark]
    public string AddPartitionKey()
    {
        return _documentId.AddPartitionKey(_partitionKey);
    }
}
