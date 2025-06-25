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
| RangeFinder_RangeQuery  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           |  71.917 μs |        NA | 0.0000 μs |  0.70 |
| IntervalTree_RangeQuery | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 102.312 μs |        NA | 0.0000 μs |  1.00 |
|                         |            |                 |                |             |              |             |            |           |           |       |
| RangeFinder_RangeQuery  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |   2.164 μs | 0.0094 μs | 0.0088 μs |  0.12 |
| IntervalTree_RangeQuery | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  17.892 μs | 0.1597 μs | 0.1494 μs |  1.00 |
