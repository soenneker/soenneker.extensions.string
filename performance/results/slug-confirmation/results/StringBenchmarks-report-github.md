```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-ZNVNAZ : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

IterationCount=5  IterationTime=100ms  LaunchCount=1
WarmupCount=3  Categories=SlugDirty

```
| Method       | Length | Mean        | Error        | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |------- |------------:|-------------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **DirtySlugOld** | **16**     |    **39.99 ns** |     **2.265 ns** |   **0.588 ns** |  **1.00** |    **0.02** | **0.0045** |      **80 B** |        **1.00** |
| DirtySlugNew | 16     |    39.61 ns |     1.603 ns |   0.416 ns |  0.99 |    0.02 | 0.0048 |      80 B |        1.00 |
|              |        |             |              |            |       |         |        |           |             |
| **DirtySlugOld** | **256**    |   **271.41 ns** |    **43.474 ns** |  **11.290 ns** |  **1.00** |    **0.05** | **0.0334** |     **560 B** |        **1.00** |
| DirtySlugNew | 256    |    60.45 ns |     1.183 ns |   0.307 ns |  0.22 |    0.01 | 0.0332 |     560 B |        1.00 |
|              |        |             |              |            |       |         |        |           |             |
| **DirtySlugOld** | **4096**   | **3,799.26 ns** | **1,170.779 ns** | **304.048 ns** |  **1.01** |    **0.10** | **0.4730** |    **8240 B** |        **1.00** |
| DirtySlugNew | 4096   |   281.65 ns |    34.099 ns |   8.855 ns |  0.07 |    0.01 | 0.4910 |    8240 B |        1.00 |
