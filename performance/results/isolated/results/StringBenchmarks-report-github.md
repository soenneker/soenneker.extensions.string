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
| **SlugOld**      | **SlugClean**  | **16**     |    **18.31 ns** |     **1.446 ns** |   **0.376 ns** |  **1.00** |    **0.03** | **0.0033** |      **56 B** |        **1.00** |
| SlugNew      | SlugClean  | 16     |    12.11 ns |     1.202 ns |   0.186 ns |  0.66 |    0.02 |      - |         - |        0.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **SlugOld**      | **SlugClean**  | **256**    |   **272.23 ns** |    **15.917 ns** |   **4.133 ns** |  **1.00** |    **0.02** | **0.0311** |     **536 B** |        **1.00** |
| SlugNew      | SlugClean  | 256    |    11.28 ns |     1.963 ns |   0.510 ns |  0.04 |    0.00 |      - |         - |        0.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **SlugOld**      | **SlugClean**  | **4096**   | **3,764.88 ns** |   **125.968 ns** |  **19.494 ns** |  **1.00** |    **0.01** | **0.4805** |    **8216 B** |        **1.00** |
| SlugNew      | SlugClean  | 4096   |   126.71 ns |    12.814 ns |   1.983 ns |  0.03 |    0.00 |      - |         - |        0.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **16**     |    **35.90 ns** |     **1.615 ns** |   **0.419 ns** |  **1.00** |    **0.02** | **0.0045** |      **80 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 16     |    32.77 ns |     5.815 ns |   1.510 ns |  0.91 |    0.04 | 0.0046 |      80 B |        1.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **256**    |   **282.21 ns** |    **18.068 ns** |   **4.692 ns** |  **1.00** |    **0.02** | **0.0328** |     **560 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 256    |   227.53 ns |    26.876 ns |   6.980 ns |  0.81 |    0.03 | 0.0317 |     560 B |        1.00 |
|              |            |        |             |              |            |       |         |        |           |             |
| **DirtySlugOld** | **SlugDirty**  | **4096**   | **3,601.25 ns** |   **433.878 ns** |  **67.143 ns** |  **1.00** |    **0.02** | **0.4632** |    **8240 B** |        **1.00** |
| DirtySlugNew | SlugDirty  | 4096   | 3,822.18 ns | 1,832.018 ns | 475.769 ns |  1.06 |    0.12 | 0.4598 |    8240 B |        1.00 |
