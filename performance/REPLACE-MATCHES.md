# ReplaceMatches — 2026-09-13

`ReplaceMatches(string, Regex, Func<ReadOnlySpan<char>, string?>)` adds computed regex replacement to the string extensions. The callback receives complete matched text as a span and runs once per match. Replacement strings can have arbitrary lengths and may depend on state, so invoking the callback twice to count and fill would change observable behavior.

The implementation combines the runtime's [span match enumerator](https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.enumeratematches?view=net-10.0) with `PooledStringBuilder` 4.0.31. Matching avoids per-match `Match` objects. The builder starts with 256 stack characters and grows only as output requires, avoiding StringBuilder's managed object and character/chunk allocations. It also avoids caching callback results in a separate collection for a subsequent exact-length allocation. The builder library itself is unchanged.

No matches returns the original input and does not invoke the callback. Null/empty replacement strings delete a match; replacements are literal and are not reprocessed. Empty matches and right-to-left regexes are supported. The latter append reversed segments and reverse the final result so callback order matches `Regex.Replace` while preserving the original order of every UTF-16 segment. Capture groups are intentionally not exposed; callers needing them should use `Regex.Replace` and `MatchEvaluator`. Regex options and timeouts come from the supplied instance. Callback and regex exceptions propagate, and pooled storage is released in `finally` through the using declaration.

## Measurements

BenchmarkDotNet, Release .NET 10, separate processes, three warmups and seven measured iterations of 150 ms. All controls share the same compiled regex, input and replacement value. Regex compilation, delegate construction and replacement-string construction occur outside measurement. `SpanStringBuilder` uses the same span enumerator and callback as the new method, isolating the construction strategy from the matching improvement.

| Workload | Regex.Replace | Span matcher + StringBuilder | ReplaceMatches |
|---|---:|---:|---:|
| 1 expansion | 126.10 ns / 440 B | 89.00 ns / 864 B | **50.17 ns / 232 B** |
| 8 redactions | 406.06 ns / 1,960 B | 280.50 ns / 760 B | **261.73 ns / 296 B** |
| 128 redactions | 5,525.35 ns / 31,000 B | 4,106.91 ns / 9,640 B | **3,910.52 ns / 4,376 B** |
| 128 removals | 5,413.74 ns / 28,440 B | **3,555.24 ns** / 4,448 B | 3,601.94 ns / **1,816 B** |

The new method reduced elapsed time by approximately 22–65% against Regex.Replace in the tested matching cases. Its separate StringBuilder control shows additional allocation reductions and usually lower execution time; the 128-removal case was about 1% slower but allocated 59% less. No-match cases all allocated zero bytes and had similar timings. Results are workload/runtime dependent. Measurements use warm pools; a cold growth rental can allocate an array. Callback-created strings or delegates add allocations outside these cached-callback workloads. Right-to-left behavior is tested for correctness, not benchmarked here.

Raw output: [CSV](results/replace-matches/results/ReplaceMatchesBenchmarks-report.csv) and [formatted report](results/replace-matches/results/ReplaceMatchesBenchmarks-report-github.md).

## Reproduce

From the existing `C:\git\Soenneker\Extensions\soenneker.extensions.string\performance` directory, prepare the repository references as in [README.md](README.md):

```powershell
python prepare.py
python prepare-builder.py
$auditTargets = (Resolve-Path ./Audit.Dependencies.targets).Path
dotnet build Performance.csproj -c Release -p:AuditLocalDependencies=true "-p:DirectoryBuildTargetsPath=$auditTargets"
$env:AuditLocalDependencies = 'true'
$env:DirectoryBuildTargetsPath = $auditTargets
try {
    dotnet run --project Performance.csproj -c Release --no-build -- --filter '*ReplaceMatchesBenchmarks*' --iterationTime 150 --warmupCount 3 --iterationCount 7 --launchCount 1 --artifacts results/replace-matches
} finally {
    Remove-Item Env:\AuditLocalDependencies,Env:\DirectoryBuildTargetsPath
}
```

The regression tests compare output with Regex.Replace across interpreted, compiled, nonbacktracking and right-to-left expressions, empty and adjacent matches, Unicode and unmatched surrogates, literal/null/expanding replacements and stack/pool boundaries. Additional checks cover no-match reference reuse, exactly-once callback order, null validation and callback failure after growth.

All 178 tests passed using the published PooledStringBuilder 4.0.31 dependency. Benchmarks used the equivalent existing builder source in `C:\git\Soenneker\Utils\soenneker.utils.pooledstringbuilders`; no builder source changes were made.
