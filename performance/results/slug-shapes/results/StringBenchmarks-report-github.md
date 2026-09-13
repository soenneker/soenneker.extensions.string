```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-ZNVNAZ : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

IterationCount=5  IterationTime=100ms  LaunchCount=1
WarmupCount=3

```
| Method       | Categories | Length | Mean        | Error        | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |----------- |------- |------------:|-------------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **SlugOld**      | **SlugClean**  | **16**     |    **18.88 ns** |     **4.289 ns** |   **1.114 ns** |  **1.00** |    **0.08** | **0.0032** |      **56 B** |        **1.00** |
| SlugNew      | SlugClean  | 16     |    12.94 ns |     2.917 ns |   0.757 ns |  0.69 |    0.05 |      - |         - |        0.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **SlugOld**      | **SlugClean**  | **256**    |   **238.26 ns** |    **15.613 ns** |   **4.055 ns** |  **1.00** |    **0.02** | **0.0300** |     **536 B** |        **1.00** |
| SlugNew      | SlugClean  | 256    |    11.93 ns |     2.974 ns |   0.772 ns |  0.05 |    0.00 |      - |         - |        0.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **SlugOld**      | **SlugClean**  | **4096**   | **3,714.27 ns** |   **116.191 ns** |  **30.174 ns** |  **1.00** |    **0.01** | **0.4603** |    **8216 B** |        **1.00** |
| SlugNew      | SlugClean  | 4096   |   137.44 ns |    21.755 ns |   5.650 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **16**     |    **38.62 ns** |    **11.348 ns** |   **2.947 ns** |  **1.00** |    **0.10** | **0.0045** |      **80 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 16     |    35.82 ns |     5.774 ns |   1.500 ns |  0.93 |    0.07 | 0.0044 |      80 B |        1.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **256**    |   **256.45 ns** |    **53.397 ns** |  **13.867 ns** |  **1.00** |    **0.07** | **0.0335** |     **560 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 256    |    63.90 ns |    17.620 ns |   4.576 ns |  0.25 |    0.02 | 0.0333 |     560 B |        1.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **4096**   | **3,391.54 ns** | **1,148.274 ns** | **177.697 ns** |  **1.00** |    **0.07** | **0.4650** |    **8240 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 4096   |   304.65 ns |    69.548 ns |  18.061 ns |  0.09 |    0.01 | 0.4909 |    8240 B |        1.00 |
