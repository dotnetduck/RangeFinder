```

BenchmarkDotNet v0.15.0, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  Job-VNGPZB : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  .NET 8.0   : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD

Runtime=.NET 8.0  

```
| Method                        | Job        | InvocationCount | IterationCount | LaunchCount | UnrollFactor | WarmupCount | Mean      | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |----------- |---------------- |--------------- |------------ |------------- |------------ |----------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| RangeFinder_QueryAllocations  | Job-VNGPZB | 1               | 1              | 1           | 1            | 1           | 33.916 μs |        NA | 0.0000 μs |  0.63 |      - |      - |   3.98 KB |        0.12 |
| IntervalTree_QueryAllocations | Job-VNGPZB | 1               | 1              | 1           | 1            | 1           | 53.416 μs |        NA | 0.0000 μs |  1.00 |      - |      - |  32.24 KB |        1.00 |
|                               |            |                 |                |             |              |             |           |           |           |       |        |        |           |             |
| RangeFinder_QueryAllocations  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  1.088 μs | 0.0025 μs | 0.0022 μs |  0.11 | 0.3986 |      - |   3.26 KB |        0.10 |
| IntervalTree_QueryAllocations | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  9.602 μs | 0.0520 μs | 0.0461 μs |  1.00 | 3.8452 | 0.0153 |  31.52 KB |        1.00 |
