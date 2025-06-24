```

BenchmarkDotNet v0.15.0, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  Job-HXLBIX : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  .NET 8.0   : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD

Runtime=.NET 8.0  

```
| Method                  | Job        | InvocationCount | IterationCount | LaunchCount | UnrollFactor | WarmupCount | Mean       | Error     | StdDev    | Ratio |
|------------------------ |----------- |---------------- |--------------- |------------ |------------- |------------ |-----------:|----------:|----------:|------:|
| RangeFinder_RangeQuery  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 125.041 μs |        NA | 0.0000 μs |  0.58 |
| IntervalTree_RangeQuery | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 216.000 μs |        NA | 0.0000 μs |  1.00 |
|                         |            |                 |                |             |              |             |            |           |           |       |
| RangeFinder_RangeQuery  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |   6.661 μs | 0.1309 μs | 0.1286 μs |  0.20 |
| IntervalTree_RangeQuery | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  33.426 μs | 0.2525 μs | 0.2109 μs |  1.00 |
