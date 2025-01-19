using System.Text;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class ToBytesBenchmark
{
    private string _value = null!;

    [GlobalSetup]
    public void Setup()
    {
        _value = "3435343";
    }

    [Benchmark(Baseline =true)]
    public byte[] ToBytesBuiltIn()
    {
        return Encoding.UTF8.GetBytes(_value);
    }

    [Benchmark]
    public byte[] ToBytes()
    {
        return _value.ToBytes();
    }
}
