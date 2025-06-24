```

BenchmarkDotNet v0.15.0, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  Job-HXLBIX : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  .NET 8.0   : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD

Runtime=.NET 8.0  

```
| Method                        | Job        | InvocationCount | IterationCount | LaunchCount | UnrollFactor | WarmupCount | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------------ |----------- |---------------- |--------------- |------------ |------------- |------------ |-----------:|----------:|----------:|------:|--------:|-------:|-------:|----------:|------------:|
| IntervalTree_QueryAllocations | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 325.021 μs |        NA | 0.0000 μs |  1.00 |    0.00 |      - |      - |  36.87 KB |        1.00 |
| RangeFinder_QueryAllocations  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 336.750 μs |        NA | 0.0000 μs |  1.04 |    0.00 |      - |      - |   3.67 KB |        0.10 |
|                               |            |                 |                |             |              |             |            |           |           |       |         |        |        |           |             |
| RangeFinder_QueryAllocations  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |   1.309 μs | 0.0120 μs | 0.0106 μs |  0.11 |    0.00 | 0.3605 |      - |   2.95 KB |        0.08 |
| IntervalTree_QueryAllocations | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  12.424 μs | 0.1706 μs | 0.1512 μs |  1.00 |    0.02 | 4.4250 | 0.0153 |  36.15 KB |        1.00 |
