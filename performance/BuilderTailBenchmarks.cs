using BenchmarkDotNet.Attributes;
using C = BuilderSnapshot.StringExtension;

[MemoryDiagnoser]
public class BuilderTailBenchmarks
{
    [Params("Digits", "Whitespace")] public string Operation { get; set; } = null!;
    [Params(32, 4096)] public int Length { get; set; }
    [Params("Mixed", "Late", "Unicode", "Clean")] public string Shape { get; set; } = null!;
    private string _text = null!;
    [GlobalSetup] public void Setup()
    {
        _text = BuilderBenchmarks.Input(Operation, Shape, Length);
        if (Current() != Builder() || Current() != Buffer()) throw new Exception("Tail mismatch");
    }
    [Benchmark(Baseline = true)] public string? Current() => Operation == "Digits" ? C.RemoveNonDigits(_text) : C.RemoveWhiteSpace(_text);
    [Benchmark] public string? Builder() => Operation == "Digits" ? C.RemoveNonDigitsBuilderTail(_text) : C.RemoveWhiteSpaceBuilderTail(_text);
    [Benchmark] public string? Buffer() => Operation == "Digits" ? C.RemoveNonDigitsBufferTail(_text) : C.RemoveWhiteSpaceBufferTail(_text);
}
