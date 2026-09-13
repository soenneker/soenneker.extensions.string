```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9445/25H2/2025Update/HudsonValley2)
AMD Ryzen Threadripper PRO 9995WX 96-Cores 2.50GHz, 1 CPU, 192 logical and 96 physical cores
.NET SDK 10.0.401
  [Host] : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

Toolchain=InProcessEmitToolchain  IterationCount=5  IterationTime=100ms
LaunchCount=1  WarmupCount=3

```
| Method | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
|------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
| Old    | 24.005 ns | 0.8439 ns | 0.1306 ns |  1.00 | 0.0033 |      56 B |        1.00 |
| New    |  8.926 ns | 1.3317 ns | 0.2061 ns |  0.37 |      - |         - |        0.00 |
