using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Logging.Abstractions;
using New = Soenneker.Extensions.String.StringExtension;
using Old = Baseline.Soenneker.Extensions.String.StringExtension;
using NewSpan = Soenneker.Extensions.Spans.Readonly.Chars.ReadOnlySpanCharExtension;
using OldSpan = Baseline.Soenneker.Extensions.Spans.Readonly.Chars.ReadOnlySpanCharExtension;
using NewByte = Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension;
using OldByte = Baseline.Soenneker.Extensions.Spans.Readonly.Bytes.ReadOnlySpanByteExtension;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class StringBenchmarks
{
    [Params(16, 256, 4096)] public int Length { get; set; }
    private string _plain = null!, _slug = null!, _dirtySlug = null!, _unicode = null!, _upper = null!;
    private IEnumerable<string> _candidates = null!;
    private string _digits = null!, _uri = null!;
    [GlobalSetup]
    public void Setup()
    {
        _plain = new string('a', Length);
        _slug = string.Join('-', Enumerable.Repeat("hello", (Length + 5) / 6));
        _dirtySlug = " Hello___WORLD!! " + _plain;
        _unicode = new string('é', Length) + '\u3000';
        _upper = new string('A', Length);
        _candidates = new List<string> { "no", "also-no", _plain };
        _digits = new string('9', Length);
        _uri = "https://example.com/" + _plain;
    }
    [Benchmark(Baseline=true), BenchmarkCategory("WhitespaceAscii")] public string? WhitespaceOld() => Old.RemoveWhiteSpace(_plain);
    [Benchmark, BenchmarkCategory("WhitespaceAscii")] public string? WhitespaceNew() => New.RemoveWhiteSpace(_plain);
    [Benchmark(Baseline=true), BenchmarkCategory("WhitespaceUnicode")] public string? UnicodeOld() => Old.RemoveWhiteSpace(_unicode);
    [Benchmark, BenchmarkCategory("WhitespaceUnicode")] public string? UnicodeNew() => New.RemoveWhiteSpace(_unicode);
    [Benchmark(Baseline=true), BenchmarkCategory("SlugClean")] public string? SlugOld() => Old.Slugify(_slug);
    [Benchmark, BenchmarkCategory("SlugClean")] public string? SlugNew() => New.Slugify(_slug);
    [Benchmark(Baseline=true), BenchmarkCategory("SlugDirty")] public string? DirtySlugOld() => Old.Slugify(_dirtySlug);
    [Benchmark, BenchmarkCategory("SlugDirty")] public string? DirtySlugNew() => New.Slugify(_dirtySlug);
    [Benchmark(Baseline=true), BenchmarkCategory("SplitSingle")] public string[]? SplitOld() => Old.SplitTrimmedNonEmpty(_plain, ',');
    [Benchmark, BenchmarkCategory("SplitSingle")] public string[]? SplitNew() => New.SplitTrimmedNonEmpty(_plain, ',');
    [Benchmark(Baseline=true), BenchmarkCategory("EqualsEnumerable")] public bool EqualsOld() => Old.EqualsAny(_plain, _candidates);
    [Benchmark, BenchmarkCategory("EqualsEnumerable")] public bool EqualsNew() => New.EqualsAny(_plain, _candidates);
    [Benchmark(Baseline=true), BenchmarkCategory("OrdinalLower")] public string LowerOld() => Old.ToLowerOrdinal(_upper);
    [Benchmark, BenchmarkCategory("OrdinalLower")] public string LowerNew() => New.ToLowerOrdinal(_upper);
    [Benchmark(Baseline=true), BenchmarkCategory("InvariantLower")] public string InvariantOld() => Old.ToLowerInvariantFast(_upper);
    [Benchmark, BenchmarkCategory("InvariantLower")] public string InvariantNew() => New.ToLowerInvariantFast(_upper);
    [Benchmark(Baseline=true), BenchmarkCategory("SecureShuffle")] public string ShuffleOld() => Old.SecureShuffle(_plain);
    [Benchmark, BenchmarkCategory("SecureShuffle")] public string ShuffleNew() => New.SecureShuffle(_plain);
    [Benchmark(Baseline=true), BenchmarkCategory("Scriban")] public string ScribanOld() => Old.ToEscapedForScriban(_plain);
    [Benchmark, BenchmarkCategory("Scriban")] public string ScribanNew() => New.ToEscapedForScriban(_plain);
    [Benchmark(Baseline=true), BenchmarkCategory("Digits")] public string? DigitsOld() => Old.RemoveNonDigits(_digits);
    [Benchmark, BenchmarkCategory("Digits")] public string? DigitsNew() => New.RemoveNonDigits(_digits);
    [Benchmark(Baseline=true), BenchmarkCategory("AlphaNumeric")] public bool AlphaOld() => Old.IsAlphaNumeric(_plain);
    [Benchmark, BenchmarkCategory("AlphaNumeric")] public bool AlphaNew() => New.IsAlphaNumeric(_plain);
    [Benchmark(Baseline=true), BenchmarkCategory("HttpLike")] public bool UriOld() => Old.IsHttpUriLike(_uri);
    [Benchmark, BenchmarkCategory("HttpLike")] public bool UriNew() => New.IsHttpUriLike(_uri);
}

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class DependencyBenchmarks
{
    [Params(16, 256, 4096)] public int Length { get; set; }
    private string _text = null!, _upper = null!, _split = null!;
    private byte[] _bytes = null!;
    private byte[] _upperBytes = null!;
    private Range[] _ranges = null!;
    [GlobalSetup]
    public void Setup()
    {
        _text = new string('a', Length);
        _upper = new string('A', Length);
        _bytes = System.Text.Encoding.UTF8.GetBytes(_text);
        _upperBytes = System.Text.Encoding.UTF8.GetBytes(_upper);
        _ranges = [0..Length, 0..Length, 0..Length];
        _split = _text + ',' + _text + ',' + _text;
    }
    [Benchmark(Baseline=true), BenchmarkCategory("Join")] public string JoinOld() => OldSpan.JoinCommaSeparated(_text, _ranges, 0, _ranges.Length);
    [Benchmark, BenchmarkCategory("Join")] public string JoinNew() => NewSpan.JoinCommaSeparated(_text, _ranges, 0, _ranges.Length);
    [Benchmark(Baseline=true), BenchmarkCategory("AsciiEquals")] public bool EqualsOld() => OldSpan.EqualsAsciiIgnoreCase_AssumeAscii(_text, _upper);
    [Benchmark, BenchmarkCategory("AsciiEquals")] public bool EqualsNew() => NewSpan.EqualsAsciiIgnoreCase_AssumeAscii(_text, _upper);
    [Benchmark(Baseline=true), BenchmarkCategory("Sha256Hex")] public string HashOld() => OldByte.ToSha256Hex(_bytes);
    [Benchmark, BenchmarkCategory("Sha256Hex")] public string HashNew() => NewByte.ToSha256Hex(_bytes);
    [Benchmark(Baseline=true), BenchmarkCategory("SplitRanges")] public int SplitOld() { Span<Range> ranges=stackalloc Range[3]; return OldSpan.SplitCommaRanges(_split,ranges); }
    [Benchmark, BenchmarkCategory("SplitRanges")] public int SplitNew() { Span<Range> ranges=stackalloc Range[3]; return NewSpan.SplitCommaRanges(_split,ranges); }
    [Benchmark(Baseline=true), BenchmarkCategory("SplitTrimmed")] public string[] TrimmedOld() => OldSpan.SplitTrimmedNonEmpty(_split, ',');
    [Benchmark, BenchmarkCategory("SplitTrimmed")] public string[] TrimmedNew() => NewSpan.SplitTrimmedNonEmpty(_split, ',');
    [Benchmark(Baseline=true), BenchmarkCategory("Classify")] public object ClassifyOld() => OldByte.Classify(_bytes);
    [Benchmark, BenchmarkCategory("Classify")] public object ClassifyNew() => NewByte.Classify(_bytes);
    [Benchmark(Baseline=true), BenchmarkCategory("AsciiBytes")] public bool BytesOld() => OldByte.Utf8AsciiEqualsIgnoreCase(_bytes, _upperBytes);
    [Benchmark, BenchmarkCategory("AsciiBytes")] public bool BytesNew() => NewByte.Utf8AsciiEqualsIgnoreCase(_bytes, _upperBytes);
    [Benchmark(Baseline=true), BenchmarkCategory("AsciiSafe")] public bool SafeOld() => OldSpan.EqualsAsciiIgnoreCase(_text, _upper);
    [Benchmark, BenchmarkCategory("AsciiSafe")] public bool SafeNew() => NewSpan.EqualsAsciiIgnoreCase(_text, _upper);
}

[MemoryDiagnoser]
public class LoggingBenchmarks
{
    [Benchmark(Baseline=true)] public ValueTask Old() => Baseline.Soenneker.Utils.Random.RandomUtil.Delay(0,0,NullLogger.Instance);
    [Benchmark] public ValueTask New() => Soenneker.Utils.Random.RandomUtil.Delay(0,0,NullLogger.Instance);
}

[MemoryDiagnoser]
public class WeightedBenchmarks
{
    [Params(4, 64, 1024)] public int Count { get; set; }
    private int[] _items=null!;
    private double[] _weights=null!;
    [GlobalSetup] public void Setup() { _items=Enumerable.Range(0,Count).ToArray(); _weights=Enumerable.Repeat(1d,Count).ToArray(); }
    [Benchmark(Baseline=true)] public int Old() => Baseline.Soenneker.Utils.Random.RandomUtil.WeightedRandomSelection(_items,_weights);
    [Benchmark] public int New() => Soenneker.Utils.Random.RandomUtil.WeightedRandomSelection(_items,_weights);
}

[MemoryDiagnoser]
public class Base64Benchmarks
{
    [Params(16, 256, 4096)] public int Length {get;set;}
    [Params(false,true)] public bool Url {get;set;}
    private string _text=null!;
    [GlobalSetup] public void Setup()
    {
        _text=Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(new string('ÿ',Length)));
        if(Url) _text=_text.Replace('+','-').Replace('/','_').TrimEnd('=');
    }
    [Benchmark(Baseline=true)] public string Old() => OldString(_text);
    private static string OldString(string value) => Baseline.Soenneker.Extensions.String.StringExtension.ToStringFromBase64(value);
    [Benchmark] public string New() => Soenneker.Extensions.String.StringExtension.ToStringFromBase64(_text);
}

[MemoryDiagnoser]
public class SlugShapeBenchmarks
{
    [Params("Words", "Unicode", "LateChange", "Uppercase", "Whitespace")]
    public string Shape { get; set; } = null!;
    [Params(32, 512, 4096)] public int Length { get; set; }
    private string _text = null!;
    [GlobalSetup] public void Setup()
    {
        string token = Shape switch
        {
            "Words" => "hello world this is a longer title ",
            "Unicode" => "École à Zürich déjà vu! ",
            "Uppercase" => "HELLOWORLD123",
            "Whitespace" => " \t\r\n",
            _ => "abc123"
        };
        _text = string.Concat(Enumerable.Repeat(token, (Length + token.Length - 1) / token.Length))[..Length];
        if (Shape == "LateChange") _text += '!';
    }
    [Benchmark(Baseline=true)] public string? Old() => Baseline.Soenneker.Extensions.String.StringExtension.Slugify(_text);
    [Benchmark] public string? New() => Soenneker.Extensions.String.StringExtension.Slugify(_text);
}
