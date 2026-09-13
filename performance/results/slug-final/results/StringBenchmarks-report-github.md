```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-ZNVNAZ : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

IterationCount=5  IterationTime=100ms  LaunchCount=1
WarmupCount=3

```
| Method       | Categories | Length | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |----------- |------- |------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **SlugOld**      | **SlugClean**  | **16**     |    **19.95 ns** |   **7.935 ns** |   **1.228 ns** |  **1.00** |    **0.08** | **0.0031** |      **56 B** |        **1.00** |
| SlugNew      | SlugClean  | 16     |    16.44 ns |   3.211 ns |   0.834 ns |  0.83 |    0.06 |      - |         - |        0.00 |
|              |            |        |             |            |            |       |         |        |           |             |
| **SlugOld**      | **SlugClean**  | **256**    |   **248.24 ns** |  **56.224 ns** |  **14.601 ns** |  **1.00** |    **0.08** | **0.0303** |     **536 B** |        **1.00** |
| SlugNew      | SlugClean  | 256    |    10.56 ns |   3.098 ns |   0.805 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|              |            |        |             |            |            |       |         |        |           |             |
| **SlugOld**      | **SlugClean**  | **4096**   | **3,349.02 ns** |  **59.930 ns** |  **15.564 ns** |  **1.00** |    **0.01** | **0.4722** |    **8216 B** |        **1.00** |
| SlugNew      | SlugClean  | 4096   |   122.56 ns |   4.378 ns |   0.678 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|              |            |        |             |            |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **16**     |    **36.42 ns** |   **7.450 ns** |   **1.935 ns** |  **1.00** |    **0.07** | **0.0046** |      **80 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 16     |    31.21 ns |   7.614 ns |   1.977 ns |  0.86 |    0.07 | 0.0044 |      80 B |        1.00 |
|              |            |        |             |            |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **256**    |   **262.83 ns** |  **58.135 ns** |  **15.097 ns** |  **1.00** |    **0.08** | **0.0311** |     **560 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 256    |    52.70 ns |  15.808 ns |   4.105 ns |  0.20 |    0.02 | 0.0332 |     560 B |        1.00 |
|              |            |        |             |            |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **4096**   | **3,333.97 ns** | **769.832 ns** | **199.923 ns** |  **1.00** |    **0.08** | **0.4674** |    **8240 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 4096   |   292.28 ns |  96.555 ns |  25.075 ns |  0.09 |    0.01 | 0.4913 |    8240 B |        1.00 |
