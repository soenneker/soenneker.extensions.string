```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Toolchain=InProcessEmitToolchain  IterationCount=5  IterationTime=100ms
LaunchCount=1  WarmupCount=3

```
| Method       | Categories     | Length | Mean        | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |--------------- |------- |------------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **InvariantOld** | **InvariantLower** | **16**     |    **12.20 ns** |   **0.655 ns** |  **0.170 ns** |  **1.00** |    **0.02** | **0.0033** |      **56 B** |        **1.00** |
| InvariantNew | InvariantLower | 16     |    10.00 ns |   0.151 ns |  0.039 ns |  0.82 |    0.01 | 0.0033 |      56 B |        1.00 |
|              |                |        |             |            |           |       |         |        |           |             |
| **InvariantOld** | **InvariantLower** | **256**    |   **137.87 ns** |   **2.438 ns** |  **0.377 ns** |  **1.00** |    **0.00** | **0.0315** |     **536 B** |        **1.00** |
| InvariantNew | InvariantLower | 256    |    29.03 ns |   1.687 ns |  0.261 ns |  0.21 |    0.00 | 0.0319 |     536 B |        1.00 |
|              |                |        |             |            |           |       |         |        |           |             |
| **InvariantOld** | **InvariantLower** | **4096**   | **1,968.66 ns** | **165.929 ns** | **43.091 ns** |  **1.00** |    **0.03** | **0.4771** |    **8216 B** |        **1.00** |
| InvariantNew | InvariantLower | 4096   |   285.54 ns |  27.088 ns |  4.192 ns |  0.15 |    0.00 | 0.4905 |    8216 B |        1.00 |
|              |                |        |             |            |           |       |         |        |           |             |
| **SlugOld**      | **SlugClean**      | **16**     |    **21.24 ns** |   **0.396 ns** |  **0.061 ns** |  **1.00** |    **0.00** | **0.0032** |      **56 B** |        **1.00** |
| SlugNew      | SlugClean      | 16     |    14.70 ns |   1.198 ns |  0.311 ns |  0.69 |    0.01 |      - |         - |        0.00 |
|              |                |        |             |            |           |       |         |        |           |             |
| **SlugOld**      | **SlugClean**      | **256**    |   **252.03 ns** |  **23.269 ns** |  **6.043 ns** |  **1.00** |    **0.03** | **0.0299** |     **536 B** |        **1.00** |
| SlugNew      | SlugClean      | 256    |    17.24 ns |   1.810 ns |  0.470 ns |  0.07 |    0.00 |      - |         - |        0.00 |
|              |                |        |             |            |           |       |         |        |           |             |
| **SlugOld**      | **SlugClean**      | **4096**   | **3,857.45 ns** |  **90.238 ns** | **23.435 ns** |  **1.00** |    **0.01** | **0.4805** |    **8216 B** |        **1.00** |
| SlugNew      | SlugClean      | 4096   |   135.70 ns |   6.809 ns |  1.768 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|              |                |        |             |            |           |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**      | **16**     |    **43.66 ns** |   **2.694 ns** |  **0.700 ns** |  **1.00** |    **0.02** | **0.0047** |      **80 B** |        **1.00** |
| DirtySlugNew | SlugDirty      | 16     |    46.34 ns |   3.158 ns |  0.820 ns |  1.06 |    0.02 | 0.0047 |      80 B |        1.00 |
|              |                |        |             |            |           |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**      | **256**    |   **274.82 ns** |  **24.383 ns** |  **6.332 ns** |  **1.00** |    **0.03** | **0.0331** |     **560 B** |        **1.00** |
| DirtySlugNew | SlugDirty      | 256    |   282.63 ns |   9.706 ns |  2.521 ns |  1.03 |    0.02 | 0.0314 |     560 B |        1.00 |
|              |                |        |             |            |           |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**      | **4096**   | **3,531.14 ns** |  **91.528 ns** | **23.770 ns** |  **1.00** |    **0.01** | **0.4672** |    **8240 B** |        **1.00** |
| DirtySlugNew | SlugDirty      | 4096   | 3,782.48 ns | 256.626 ns | 66.645 ns |  1.07 |    0.02 | 0.4796 |    8240 B |        1.00 |
