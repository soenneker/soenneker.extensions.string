```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Toolchain=InProcessEmitToolchain  IterationCount=5  IterationTime=100ms
LaunchCount=1  WarmupCount=3

```
| Method | Count | Mean        | Error     | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|------- |------ |------------:|----------:|---------:|------:|--------:|----------:|------------:|
| **Old**    | **4**     |    **23.30 ns** |  **0.605 ns** | **0.157 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| New    | 4     |    16.69 ns |  1.315 ns | 0.342 ns |  0.72 |    0.01 |         - |          NA |
|        |       |             |           |          |       |         |           |             |
| **Old**    | **64**    |   **199.08 ns** | **14.022 ns** | **3.641 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| New    | 64    |    86.32 ns |  2.261 ns | 0.587 ns |  0.43 |    0.01 |         - |          NA |
|        |       |             |           |          |       |         |           |             |
| **Old**    | **1024**  | **2,354.95 ns** | **10.285 ns** | **1.592 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| New    | 1024  | 1,350.23 ns | 19.231 ns | 2.976 ns |  0.57 |    0.00 |         - |          NA |
