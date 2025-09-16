using BenchmarkDotNet.Attributes;
using System;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class AddDocumentIdBenchmark
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
    public string AddDocumentId()
    {
        return _partitionKey.AddDocumentId(_documentId);
    }
}
