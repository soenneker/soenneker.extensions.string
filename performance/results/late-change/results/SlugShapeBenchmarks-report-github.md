```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-ZNVNAZ : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

IterationCount=5  IterationTime=100ms  LaunchCount=1
WarmupCount=3

```
| Method | Shape      | Length | Mean        | Error      | StdDev     | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------- |----------- |------- |------------:|-----------:|-----------:|------:|--------:|-------:|----------:|------------:|
| **Old**    | **LateChange** | **32**     |    **30.65 ns** |   **0.937 ns** |   **0.243 ns** |  **1.00** |    **0.01** | **0.0052** |      **88 B** |        **1.00** |
| New    | LateChange | 32     |    28.19 ns |   3.632 ns |   0.562 ns |  0.92 |    0.02 | 0.0050 |      88 B |        1.00 |
|        |            |        |             |            |            |       |         |        |           |             |
| **Old**    | **LateChange** | **512**    |   **467.04 ns** | **121.664 ns** |  **31.596 ns** |  **1.00** |    **0.09** | **0.0599** |    **1048 B** |        **1.00** |
| New    | LateChange | 512    |    77.21 ns |  23.202 ns |   6.026 ns |  0.17 |    0.02 | 0.0625 |    1048 B |        1.00 |
|        |            |        |             |            |            |       |         |        |           |             |
| **Old**    | **LateChange** | **4096**   | **3,480.33 ns** | **778.419 ns** | **202.153 ns** |  **1.00** |    **0.08** | **0.4825** |    **8216 B** |        **1.00** |
| New    | LateChange | 4096   |   442.73 ns |  37.972 ns |   9.861 ns |  0.13 |    0.01 | 0.4896 |    8216 B |        1.00 |
