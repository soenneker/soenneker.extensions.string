```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Toolchain=InProcessEmitToolchain  IterationCount=5  IterationTime=100ms
LaunchCount=1  WarmupCount=3

```
| Method | Count | Mean        | Error     | StdDev    | Ratio | Allocated | Alloc Ratio |
|------- |------ |------------:|----------:|----------:|------:|----------:|------------:|
| **Old**    | **4**     |    **22.70 ns** |  **0.285 ns** |  **0.074 ns** |  **1.00** |         **-** |          **NA** |
| New    | 4     |    15.93 ns |  0.207 ns |  0.054 ns |  0.70 |         - |          NA |
|        |       |             |           |           |       |           |             |
| **Old**    | **64**    |   **189.35 ns** |  **4.316 ns** |  **0.668 ns** |  **1.00** |         **-** |          **NA** |
| New    | 64    |    84.66 ns |  2.357 ns |  0.612 ns |  0.45 |         - |          NA |
|        |       |             |           |           |       |           |             |
| **Old**    | **1024**  | **2,331.93 ns** | **56.738 ns** | **14.735 ns** |  **1.00** |         **-** |          **NA** |
| New    | 1024  | 1,396.06 ns | 38.043 ns |  9.880 ns |  0.60 |         - |          NA |
