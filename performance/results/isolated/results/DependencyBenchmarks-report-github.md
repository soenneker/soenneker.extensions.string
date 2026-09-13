```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-ZNVNAZ : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

IterationCount=5  IterationTime=100ms  LaunchCount=1
WarmupCount=3  Categories=SplitTrimmed

```
| Method     | Length | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------- |------- |------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **TrimmedOld** | **16**     |    **38.22 ns** |   **3.061 ns** |   **0.474 ns** |  **1.00** |    **0.02** | **0.0126** |      **-** |     **216 B** |        **1.00** |
| TrimmedNew | 16     |    33.33 ns |   5.679 ns |   1.475 ns |  0.87 |    0.04 | 0.0126 |      - |     216 B |        1.00 |
|            |        |             |            |            |       |         |        |        |           |             |
| **TrimmedOld** | **256**    |   **336.01 ns** |   **7.056 ns** |   **1.092 ns** |  **1.00** |    **0.00** | **0.0979** |      **-** |    **1656 B** |        **1.00** |
| TrimmedNew | 256    |    73.82 ns |  26.201 ns |   6.804 ns |  0.22 |    0.02 | 0.0987 |      - |    1656 B |        1.00 |
|            |        |             |            |            |       |         |        |        |           |             |
| **TrimmedOld** | **4096**   | **3,657.84 ns** | **229.681 ns** |  **59.648 ns** |  **1.00** |    **0.02** | **1.4752** | **0.0776** |   **24696 B** |        **1.00** |
| TrimmedNew | 4096   |   843.82 ns | 453.124 ns | 117.675 ns |  0.23 |    0.03 | 1.4737 | 0.0875 |   24696 B |        1.00 |
