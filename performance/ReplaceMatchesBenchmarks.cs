using System.Text;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using Soenneker.Extensions.String;

[MemoryDiagnoser]
public class ReplaceMatchesBenchmarks
{
    [Params(0, 1, 8, 128)] public int MatchCount { get; set; }
    [Params("Redact", "Expand", "Remove")] public string Shape { get; set; } = null!;
    private string _input = null!;
    private Regex _regex = null!;
    private MatchEvaluator _matchEvaluator = null!;
    private Func<ReadOnlySpan<char>, string?> _spanEvaluator = null!;

    [GlobalSetup]
    public void Setup()
    {
        _input = MatchCount == 0 ? "no matching identifiers here" : string.Concat(Enumerable.Repeat("item=A12; ", MatchCount));
        _regex = new Regex("[A-Z][0-9]{2}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        string? replacement = Shape switch { "Expand" => new string('x', 96), "Remove" => null, _ => "[redacted]" };
        _matchEvaluator = _ => replacement!;
        _spanEvaluator = _ => replacement;
        if (PooledBuilder() != RuntimeReplace() || SpanStringBuilder() != RuntimeReplace()) throw new Exception("Replacement benchmark mismatch");
    }

    [Benchmark(Baseline = true)] public string RuntimeReplace() => _regex.Replace(_input, _matchEvaluator);

    [Benchmark] public string PooledBuilder() => _input.ReplaceMatches(_regex, _spanEvaluator);

    [Benchmark] public string SpanStringBuilder()
    {
        Regex.ValueMatchEnumerator matches = _regex.EnumerateMatches(_input);
        if (!matches.MoveNext()) return _input;
        var builder = new StringBuilder(_input.Length);
        int position = 0;
        do
        {
            ValueMatch match = matches.Current;
            builder.Append(_input.AsSpan(position, match.Index - position));
            builder.Append(_spanEvaluator(_input.AsSpan(match.Index, match.Length)));
            position = match.Index + match.Length;
        } while (matches.MoveNext());
        builder.Append(_input.AsSpan(position));
        return builder.ToString();
    }
}
