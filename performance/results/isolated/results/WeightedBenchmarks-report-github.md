```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-ZNVNAZ : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

IterationCount=5  IterationTime=100ms  LaunchCount=1
WarmupCount=3

```
| Method | Count | Mean        | Error      | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------- |------ |------------:|-----------:|----------:|------:|--------:|----------:|------------:|
| **Old**    | **4**     |    **21.79 ns** |   **1.887 ns** |  **0.490 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| New    | 4     |    14.67 ns |   0.224 ns |  0.035 ns |  0.67 |    0.01 |         - |          NA |
|        |       |             |            |           |       |         |           |             |
| **Old**    | **64**    |   **203.99 ns** |  **11.619 ns** |  **3.017 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| New    | 64    |    85.79 ns |  17.520 ns |  4.550 ns |  0.42 |    0.02 |         - |          NA |
|        |       |             |            |           |       |         |           |             |
| **Old**    | **1024**  | **2,994.81 ns** | **289.563 ns** | **75.199 ns** |  **1.00** |    **0.03** |         **-** |          **NA** |
| New    | 1024  | 1,455.80 ns | 371.813 ns | 96.559 ns |  0.49 |    0.03 |         - |          NA |
