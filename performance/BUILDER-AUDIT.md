# PooledStringBuilder follow-up

This report covers the methods that existed during the audit. The later [ReplaceMatches addition](REPLACE-MATCHES.md) introduces a new use for the builder; it does not replace the implementations evaluated here.

This follow-up evaluates whether the updated PooledStringBuilder belongs in `Soenneker.Extensions.String`. The string control is commit `99cbac4`, after the first performance audit. Builder source comes from `fa0f48c`; its production implementation is the optimization in `13789a7`, released as `4.0.31`. The builder repository's implementation is not modified here.

## Comparisons

`BuilderBenchmarks` compares the following with identical inputs and output semantics:

- `Current`: pinned post-audit string implementation.
- `Builder`: one-pass incremental append, with stack storage through 512 characters and pre-sized pooled storage above that. Slug generation uses `AppendSpan` to preserve its existing specialized writer.
- `Buffer`: a one-pass stack/pooled span implementation, to separate the benefit of removing a counting pass from the builder abstraction itself.
- `BuilderSpan`: reserves the upper bound once with `AppendSpan`, writes directly, then shrinks to the actual length. Used for digit/whitespace/character removal and Scriban escaping.

For operations without a distinct control, `Buffer` delegates to `Current` (phone sanitization and slug generation), and `BuilderSpan` delegates to `Builder` (phone sanitization, phone URI formatting, newline replacement and slug generation). These duplicate controls are not separate implementations. In the initial exploration, the Tel buffer and Scriban span variants had not yet been introduced, so those rows also delegate to the existing controls.

The source scan covered every string-producing method. Variable-output candidates were digit removal, whitespace removal, character removal (also used by `RemoveDashes`), Scriban escaping, slugs, phone sanitization, phone URI formatting (`tel:` and `sms:` share the implementation), and CRLF replacement. Other methods return slices, build fixed-length results, use `string.Concat`, produce collections of separate strings, or delegate encoding/escaping/parsing to the runtime. There is no general StringBuilder append loop left to replace. Casing, masking and display-phone formatting already write into their final string; binary encodings need byte buffers rather than a character builder.

## Reproduction

Use the existing repository layout and .NET 10 setup in [README.md](README.md). Both the categorized `C:\git\Soenneker` layout and sibling checkouts are supported. Prepare both comparison generations:

```powershell
python prepare.py
python prepare-builder.py
$auditTargets = (Resolve-Path ./Audit.Dependencies.targets).Path
dotnet run --project Performance.csproj -c Release -p:AuditLocalDependencies=true "-p:DirectoryBuildTargetsPath=$auditTargets" -- --verify-builder
```

The original exploration used seven operations (all except `Newlines`), lengths 32, 512 and 4096, and mixed text. It used the in-process toolchain. For the separate-process confirmation:

```powershell
$env:AuditLocalDependencies = 'true'
$env:DirectoryBuildTargetsPath = (Resolve-Path ./Audit.Dependencies.targets).Path
$env:BuilderOperations = 'Digits,Whitespace,Scriban,Tel,Newlines'
$env:BuilderLengths = '32,4096'
$env:BuilderShapes = 'Mixed,Clean,Late,Unicode'
try {
    dotnet run --project Performance.csproj -c Release --no-build -- --filter '*BuilderBenchmarks*' --iterationTime 100 --warmupCount 3 --iterationCount 5 --launchCount 1 --artifacts results/builder-confirmation
} finally {
    Remove-Item Env:\AuditLocalDependencies,Env:\DirectoryBuildTargetsPath,Env:\BuilderOperations,Env:\BuilderLengths,Env:\BuilderShapes
}
```

`Mixed` contains repeated changes, `Clean` takes no-change paths where the API has one, `Late` changes the end of the input, and `Unicode` includes non-ASCII whitespace/digits, accents and brace pairs. A clean phone URI still needs its prefix and country code.

These are steady-state microbenchmarks on one runtime and CPU, with warm pools. They do not measure cold rentals or pool-retained memory. A builder still allocates the returned nonempty string; equal allocation columns do not mean the intermediate storage is free.

## Decision and retained change

None of the existing methods evaluated here justified introducing PooledStringBuilder. It can beat existing two-pass implementations on selected inputs, but direct span controls generally do better, and the builder alternatives regress other inputs. The builder implementation remains unchanged.

Retain the one-pass **Scriban escaping** change using a direct stack/pooled span. The public no-change check stays outside the buffer-writing helper. Existing brace-pair removal, Unicode whitespace normalization, null/empty behavior and input-reference reuse are preserved. In the final separate-process comparison, changed inputs took 15–48% less time, with the same steady-state managed allocations.

| Scriban input | Length | Before | Retained implementation | Allocation, both |
|---|---:|---:|---:|---:|
| Mixed | 16 | 31.26 ns | 24.15 ns | 48 B |
| Mixed | 512 | 1,019.54 ns | 597.02 ns | 896 B |
| Mixed | 4096 | 7,890.22 ns | 4,675.73 ns | 7,000 B |
| Unicode | 4096 | 7,968.30 ns | 5,589.82 ns | 6,856 B |
| Late change | 4096 | 8,215.83 ns | 4,264.08 ns | 8,216 B |

Source: `results/builder-integrated/results/BuilderBenchmarks-report.csv`, **Scriban rows only**. Clean inputs continue to reuse the original string without allocating. The retained implementation introduces an input-sized pooled temporary for large changed inputs where the previous implementation wrote directly into the result. A cold rental can allocate an extra array, and the shared pool can retain it. This is a throughput improvement with a temporary-storage tradeoff, not an unconditional allocation improvement.

## Rejected alternatives

| Area | Evidence and decision |
|---|---|
| Slugs | Builder storage around the existing specialized writer was slower at all three mixed-text sizes in exploration. Retain existing stack/pool code and no-change paths. |
| Character removal | The vectorized count plus direct result write beat the builder at all three mixed-text sizes. Retain it, including `RemoveDashes`. |
| Phone sanitization | Short/medium inputs slowed down with the builder. The small long-input difference did not justify replacement. |
| Phone URI formatting | A one-pass direct-span candidate beat the builder. However, integration into the shared `tel:`/`sms:` helper slowed typical 16-character inputs by about 1–2 ns (6–10%), despite improving long inputs. The production change was reverted. **Tel Integrated rows are a withdrawn experiment**, not the final production implementation. |
| Digit/whitespace filters | One-pass candidates often improved mixed text, but prefix buffering regressed late changes. Tail-only buffering removes that extra prefix copy, yet did not establish a consistent builder advantage over direct spans. |
| Growing filters | Stack storage avoids a large rental when most input is discarded. However, at 65,536 characters the growing builder slowed mixed filtering by 7–9%, and all-removed filters by 36–76%. Retain the existing filters. |
| CRLF replacement | Repeated CRLF text improved, but a single late replacement regressed. Retain the runtime implementation. |

## Additional experiments and reproducibility

The stored runs contain 376 timings: 84 initial in-process comparisons, 160 separate-process confirmations, 48 tail-only filter comparisons, 36 growing filter comparisons, and 48 production integration comparisons. Repeated controls and small differences should not be treated as independent wins. The final production source retains only the Scriban change; historical candidate rows intentionally include rejected implementations.

`BuilderTailBenchmarks` copies the unchanged prefix directly into the final string and buffers only the remainder. `BuilderGrowthBenchmarks` starts with at most 512 stack characters and grows only as output is appended; its sparse and entirely removed inputs extend to 65,536 characters.

```powershell
# Set AuditLocalDependencies and DirectoryBuildTargetsPath in the environment as above.
dotnet run --project Performance.csproj -c Release --no-build -- --filter '*BuilderTailBenchmarks*' --iterationTime 100 --warmupCount 3 --iterationCount 5 --launchCount 1 --artifacts results/builder-tail
dotnet run --project Performance.csproj -c Release --no-build -- --filter '*BuilderGrowthBenchmarks*' --iterationTime 100 --warmupCount 3 --iterationCount 5 --launchCount 1 --artifacts results/builder-growth
```

For final Scriban integration, set `BuilderOperations=Scriban`, `BuilderLengths=16,512,4096`, and `BuilderShapes=Mixed,Clean,Late,Unicode`; filter `*BuilderBenchmarks.Current*` and `*BuilderBenchmarks.Integrated*` with the same job settings. The original integration run also measured the subsequently rejected Tel implementation.

The cold probe uses a fresh process per variant and disables tiered compilation to avoid tier-transition effects on allocation counts. It warms small transforms and long no-change searches, leaving the large character-array pool bucket cold:

```powershell
$env:DOTNET_TieredCompilation = '0'
try {
    foreach ($variant in @('Current', 'Builder', 'Buffer')) {
        dotnet run --project Performance.csproj -c Release --no-build -- --cold-builder $variant
    }
} finally {
    Remove-Item Env:\DOTNET_TieredCompilation
}
```

For a 65,536-character input yielding 64 digits, the existing filter and growing builder each allocated 152 B. The input-sized span candidate allocated 133,256 B on the first changed call, then 152 B on the repeated call. The first-call measurement includes pool initialization as well as the array. This supports retaining the current filters rather than treating a warm-pool speedup as a universal allocation improvement.

## Validation

171 MTP/TUnit tests passed, including new Scriban and phone URI regression checks. The focused new tests also passed against published dependencies. The differential suite passed **3,106,260 checks**, both with hardware intrinsics enabled and disabled, against the pinned post-audit implementations. Coverage includes every UTF-16 character, malformed surrogates, brace pairs, Unicode digits/whitespace, null/empty inputs, negative/minimum/maximum country codes, stack/pool transitions, sparse/removed outputs and both phone URI prefixes.

When switching between published dependencies and the local source graph, allow restore to run again; `--no-restore` can leave incompatible package/project assets from the other mode.
