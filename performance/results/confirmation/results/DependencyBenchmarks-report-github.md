```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Toolchain=InProcessEmitToolchain  IterationCount=5  IterationTime=100ms
LaunchCount=1  WarmupCount=3  Categories=AsciiBytes

```
| Method   | Length | Mean         | Error       | StdDev     | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------- |------- |-------------:|------------:|-----------:|------:|--------:|----------:|------------:|
| **BytesOld** | **16**     |     **8.210 ns** |   **0.0634 ns** |  **0.0165 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| BytesNew | 16     |     4.208 ns |   0.0777 ns |  0.0202 ns |  0.51 |    0.00 |         - |          NA |
|          |        |              |             |            |       |         |           |             |
| **BytesOld** | **256**    |   **128.730 ns** |   **3.3951 ns** |  **0.8817 ns** |  **1.00** |    **0.01** |         **-** |          **NA** |
| BytesNew | 256    |     8.925 ns |   0.7263 ns |  0.1886 ns |  0.07 |    0.00 |         - |          NA |
|          |        |              |             |            |       |         |           |             |
| **BytesOld** | **4096**   | **1,946.418 ns** | **134.1678 ns** | **34.8429 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| BytesNew | 4096   |   121.723 ns |   2.9055 ns |  0.4496 ns |  0.06 |    0.00 |         - |          NA |
