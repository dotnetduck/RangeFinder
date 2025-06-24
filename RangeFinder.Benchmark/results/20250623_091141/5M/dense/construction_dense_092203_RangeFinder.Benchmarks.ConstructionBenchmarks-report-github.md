```

BenchmarkDotNet v0.15.0, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  Job-HXLBIX : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  .NET 8.0   : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD

Runtime=.NET 8.0  

```
| Method                    | Job        | InvocationCount | IterationCount | LaunchCount | UnrollFactor | WarmupCount | Mean       | Error    | StdDev   | Ratio |
|-------------------------- |----------- |---------------- |--------------- |------------ |------------- |------------ |-----------:|---------:|---------:|------:|
| RangeFinder_Construction  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           |   851.3 ms |       NA |  0.00 ms |  0.09 |
| IntervalTree_Construction | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 9,786.8 ms |       NA |  0.00 ms |  1.00 |
|                           |            |                 |                |             |              |             |            |          |          |       |
| RangeFinder_Construction  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |   842.5 ms |  2.34 ms |  2.08 ms |  0.09 |
| IntervalTree_Construction | .NET 8.0   | Default         | Default        | Default     | 16           | Default     | 9,752.4 ms | 52.87 ms | 49.45 ms |  1.00 |
