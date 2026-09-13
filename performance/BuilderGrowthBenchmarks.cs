using BenchmarkDotNet.Attributes;
using C = BuilderSnapshot.StringExtension;

[MemoryDiagnoser]
public class BuilderGrowthBenchmarks
{
    [Params("Digits", "Whitespace")] public string Operation { get; set; } = null!;
    [Params(4096, 65536)] public int Length { get; set; }
    [Params("Mixed", "Sparse", "Removed")] public string Shape { get; set; } = null!;
    private string _text = null!;
    [GlobalSetup] public void Setup()
    {
        _text = BuilderBenchmarks.Input(Operation, Shape, Length);
        if (Current() != Builder() || Current() != Buffer()) throw new Exception("Growth mismatch");
    }
    [Benchmark(Baseline = true)] public string? Current() => Operation == "Digits" ? C.RemoveNonDigits(_text) : C.RemoveWhiteSpace(_text);
    [Benchmark] public string? Builder() => Operation == "Digits" ? C.RemoveNonDigitsBuilderGrowTail(_text) : C.RemoveWhiteSpaceBuilderGrowTail(_text);
    [Benchmark] public string? Buffer() => Operation == "Digits" ? C.RemoveNonDigitsBufferTail(_text) : C.RemoveWhiteSpaceBufferTail(_text);
}
