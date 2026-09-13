# Performance audit

The later [ReplaceMatches benchmark](REPLACE-MATCHES.md) measures the new method's use of PooledStringBuilder against Regex.Replace and a span matcher with StringBuilder.

The audit compares the original source revisions in [baseline.json](baseline.json) with the local changes. Baselines use a separate namespace and the same runtime. Exploration uses in-process timing, with additional isolated-process confirmation. `Baseline/` is generated and ignored. The builder source is needed only to compile the original span-join implementation; `PooledStringBuilder` is excluded from changes at the user's request.

## Reproduce

Use .NET 10 and Python 3. The runner locates the existing categorized repositories under `C:\git\Soenneker` (for example `Extensions`, `Utils`, and `Hashing`); it also supports sibling checkouts. The recorded commits in `baseline.json` must be available in their Git histories. Run from the existing string repository's `performance` directory:

```powershell
python prepare.py
python prepare-builder.py
$auditTargets = (Resolve-Path ./Audit.Dependencies.targets).Path
dotnet run --project Performance.csproj -c Release -p:AuditLocalDependencies=true "-p:DirectoryBuildTargetsPath=$auditTargets" -- --verify
dotnet run --project Performance.csproj -c Release --no-build -- --filter '*' --inProcess --iterationTime 100 --warmupCount 3 --iterationCount 5 --launchCount 1 --artifacts results/reproduction
```

`prepare.py` generates ignored `Audit.Projects.props` and `Audit.Dependencies.targets` inside this directory, using absolute paths to the existing projects. It does not write shared build configuration above the repository. Ordinary builds continue using published NuGet references. Do not publish with the audit property enabled.

For isolated benchmark processes, pass the dependency property through the environment so BenchmarkDotNet's generated project builds use the same source graph:

```powershell
$env:AuditLocalDependencies = 'true'
$env:DirectoryBuildTargetsPath = (Resolve-Path ./Audit.Dependencies.targets).Path
try {
    dotnet run --project Performance.csproj -c Release --no-build -- --filter '*Slug*' '*Trimmed*' '*LoggingBenchmarks*' '*WeightedBenchmarks*' --iterationTime 100 --warmupCount 3 --iterationCount 5 --launchCount 1 --artifacts results/isolated-reproduction
} finally {
    Remove-Item Env:\AuditLocalDependencies,Env:\DirectoryBuildTargetsPath
}
```

Both environment properties propagate to BenchmarkDotNet's generated processes and dependency builds. The generated absolute paths remain local and are not committed.

To check the scalar fallback:

```powershell
$env:DOTNET_EnableHWIntrinsic = '0'
try {
    dotnet run --project Performance.csproj -c Release --no-build -- --verify
} finally {
    Remove-Item Env:\DOTNET_EnableHWIntrinsic
}
```

The suite includes original implementations as controls, including some candidates that were ultimately rejected. Do not interpret every `New` row as a changed method. Use [AUDIT.md](AUDIT.md) to identify retained changes and the applicable measurement run.

The later [PooledStringBuilder comparison](BUILDER-AUDIT.md) uses its updated stack-storage API. That suite requires builder source `fa0f48c` (production optimization `13789a7`, package 4.0.31), and `prepare-builder.py` generates the post-audit string baseline. Its `BuilderBenchmarks` results are separate from the original audit's accepted results.

## Methodology and limits

- BenchmarkDotNet 0.15.8, Release, .NET 10.0.12, Windows x64, Ryzen Threadripper PRO 9995WX.
- Five measured iterations of approximately 100 ms, three warmups, one launch. Exploration uses the in-process emit toolchain; selected results are confirmed using the default toolchain with separate benchmark processes. No parallel benchmark or test workloads were intentionally run during timed measurements.
- Each pair uses the same inputs. String and span workloads use size parameters 16, 256, and 4096; weighted selection covers 4, 64, and 1024 items. Additional slug cases cover prose, Unicode, uppercase identifiers, whitespace and late changes at 32, 512 and 4096 characters.
- Reported allocations are steady-state managed allocations, including returned strings/arrays. They exclude one-time static initialization and do not measure native allocations or cold pool rentals. In particular, removing a pooled intermediate improves copying and retained memory even when the allocation column stays unchanged.
- Microbenchmarks on one CPU/runtime do not prove a universal optimum. Small timing differences and polymorphic dispatch results are especially sensitive to tiered compilation and dynamic PGO. No claim is made about end-to-end application latency or other architectures.
- `--verify` performs deterministic differential checks, checks all UTF-16 code units for relevant classification operations, and exercises malformed input, Unicode, vector boundaries, pooled paths, and reference reuse. These checks complement the repositories' MTP/TUnit tests.

## Release dependencies

Local source integration was tested with opt-in project references. To ship all improvements, publish byte-span and random changes first, update/release the character-span package against the byte-span version, then update/release the string package against the new character-span and random versions. Character-span no longer needs the pooled-builder package. No NuGet version numbers have been invented and no packages have been published by this audit.
