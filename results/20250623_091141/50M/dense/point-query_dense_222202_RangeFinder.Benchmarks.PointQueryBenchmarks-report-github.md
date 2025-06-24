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
| RangeFinder_PointQuery  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 114.459 μs |        NA | 0.0000 μs |  0.74 |
| IntervalTree_PointQuery | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 154.770 μs |        NA | 0.0000 μs |  1.00 |
|                         |            |                 |                |             |              |             |            |           |           |       |
| RangeFinder_PointQuery  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |   2.588 μs | 0.0202 μs | 0.0179 μs |  0.12 |
| IntervalTree_PointQuery | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  21.371 μs | 0.1600 μs | 0.1497 μs |  1.00 |
