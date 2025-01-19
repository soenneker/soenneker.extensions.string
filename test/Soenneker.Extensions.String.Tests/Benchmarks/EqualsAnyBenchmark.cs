using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class EqualsAnyBenchmark
{
    private List<string> _strings = null!;
    private string _value = null!;

    /// <summary>
    /// The number of strings in the collection to benchmark with.
    /// </summary>
    [Params(10, 100, 1000, 10000)]
    public int Count;

    /// <summary>
    /// Setup method to initialize data before benchmarking.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Initialize the list with distinct strings
        _strings = Enumerable.Range(0, Count).Select(i => "String" + i).ToList();

        // Set the value to a string that exists in the middle of the list
        _value = "String" + Count / 2;
    }

    /// <summary>
    /// Benchmark for the custom EqualsAny extension method.
    /// </summary>
    [Benchmark]
    public bool EqualsAny()
    {
        return _value.EqualsAny(_strings);
    }
}
