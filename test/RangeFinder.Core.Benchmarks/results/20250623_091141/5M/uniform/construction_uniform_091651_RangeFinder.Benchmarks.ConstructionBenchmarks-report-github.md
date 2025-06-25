```

BenchmarkDotNet v0.15.0, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  Job-HXLBIX : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  .NET 8.0   : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD

Runtime=.NET 8.0  

```
| Method                    | Job        | InvocationCount | IterationCount | LaunchCount | UnrollFactor | WarmupCount | Mean        | Error    | StdDev   | Ratio |
|-------------------------- |----------- |---------------- |--------------- |------------ |------------- |------------ |------------:|---------:|---------:|------:|
| RangeFinder_Construction  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           |    853.6 ms |       NA |  0.00 ms |  0.08 |
| IntervalTree_Construction | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 10,432.0 ms |       NA |  0.00 ms |  1.00 |
|                           |            |                 |                |             |              |             |             |          |          |       |
| RangeFinder_Construction  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |    834.2 ms |  3.07 ms |  2.72 ms |  0.08 |
| IntervalTree_Construction | .NET 8.0   | Default         | Default        | Default     | 16           | Default     | 10,375.3 ms | 78.36 ms | 69.46 ms |  1.00 |
