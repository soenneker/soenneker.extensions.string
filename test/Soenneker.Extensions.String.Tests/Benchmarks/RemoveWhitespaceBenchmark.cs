using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class RemoveWhiteSpaceBenchmark
{
    private string _value = null!;

    [GlobalSetup]
    public void Setup()
    {
        _value = " 34352342arstA  ";
    }

    [Benchmark(Baseline =true)]
    public string RemoveWhiteSpaceBuiltIn()
    {
        return string.Concat(_value.Where(c => !char.IsWhiteSpace(c)));
    }

    [Benchmark]
    public string RemoveWhiteSpace()
    {
        return _value.RemoveWhiteSpace();
    }
}
