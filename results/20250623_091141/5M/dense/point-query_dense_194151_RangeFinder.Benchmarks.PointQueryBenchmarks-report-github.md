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
| RangeFinder_PointQuery  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           |  85.187 μs |        NA | 0.0000 μs |  0.69 |
| IntervalTree_PointQuery | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 123.041 μs |        NA | 0.0000 μs |  1.00 |
|                         |            |                 |                |             |              |             |            |           |           |       |
| RangeFinder_PointQuery  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |   2.561 μs | 0.0172 μs | 0.0161 μs |  0.14 |
| IntervalTree_PointQuery | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  18.735 μs | 0.0688 μs | 0.0643 μs |  1.00 |
