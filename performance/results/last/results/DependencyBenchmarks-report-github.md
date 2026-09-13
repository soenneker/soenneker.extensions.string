```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Toolchain=InProcessEmitToolchain  IterationCount=5  IterationTime=100ms
LaunchCount=1  WarmupCount=3  Categories=SplitTrimmed

```
| Method     | Length | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|----------- |------- |------------:|-----------:|-----------:|------:|--------:|-------:|-------:|----------:|------------:|
| **TrimmedOld** | **16**     |    **52.54 ns** |   **1.688 ns** |   **0.438 ns** |  **1.00** |    **0.01** | **0.0128** |      **-** |     **216 B** |        **1.00** |
| TrimmedNew | 16     |    43.48 ns |   2.208 ns |   0.573 ns |  0.83 |    0.01 | 0.0126 |      - |     216 B |        1.00 |
|            |        |             |            |            |       |         |        |        |           |             |
| **TrimmedOld** | **256**    |   **459.29 ns** |  **24.309 ns** |   **6.313 ns** |  **1.00** |    **0.02** | **0.0946** |      **-** |    **1656 B** |        **1.00** |
| TrimmedNew | 256    |    86.35 ns |   8.429 ns |   2.189 ns |  0.19 |    0.00 | 0.0983 |      - |    1656 B |        1.00 |
|            |        |             |            |            |       |         |        |        |           |             |
| **TrimmedOld** | **4096**   | **5,910.60 ns** | **538.141 ns** | **139.753 ns** |  **1.00** |    **0.03** | **1.4454** | **0.0578** |   **24696 B** |        **1.00** |
| TrimmedNew | 4096   |   686.26 ns |  63.099 ns |  16.387 ns |  0.12 |    0.00 | 1.4739 | 0.0840 |   24696 B |        1.00 |
