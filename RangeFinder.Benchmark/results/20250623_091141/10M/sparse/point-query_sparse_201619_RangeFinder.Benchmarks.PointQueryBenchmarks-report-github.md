```

BenchmarkDotNet v0.15.0, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  Job-HXLBIX : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  .NET 8.0   : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD

Runtime=.NET 8.0  

```
| Method                  | Job        | InvocationCount | IterationCount | LaunchCount | UnrollFactor | WarmupCount | Mean        | Error    | StdDev   | Ratio |
|------------------------ |----------- |---------------- |--------------- |------------ |------------- |------------ |------------:|---------:|---------:|------:|
| RangeFinder_PointQuery  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 56,000.0 ns |       NA |  0.00 ns |  0.99 |
| IntervalTree_PointQuery | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 56,583.5 ns |       NA |  0.00 ns |  1.00 |
|                         |            |                 |                |             |              |             |             |          |          |       |
| RangeFinder_PointQuery  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |    774.4 ns |  1.05 ns |  0.98 ns |  0.11 |
| IntervalTree_PointQuery | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  7,039.2 ns | 51.86 ns | 45.97 ns |  1.00 |
