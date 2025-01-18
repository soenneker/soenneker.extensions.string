using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class RemoveWhitespaceBenchmark
{
    private string _value;

    [GlobalSetup]
    public void Setup()
    {
        _value = " 34352342arstA  ";
    }

    [Benchmark(Baseline =true)]
    public string RemoveWhitespaceBuiltIn()
    {
        return string.Concat(_value.Where(c => !char.IsWhiteSpace(c)));
    }

    [Benchmark]
    public string RemoveWhitespace()
    {
        return _value.RemoveWhitespace();
    }
}
