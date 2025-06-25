```

BenchmarkDotNet v0.15.0, macOS Sequoia 15.5 (24F74) [Darwin 24.5.0]
Apple M4, 1 CPU, 10 logical and 10 physical cores
.NET SDK 8.0.300
  [Host]     : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  Job-HXLBIX : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD
  .NET 8.0   : .NET 8.0.5 (8.0.524.21615), Arm64 RyuJIT AdvSIMD

Runtime=.NET 8.0  

```
| Method                    | Job        | InvocationCount | IterationCount | LaunchCount | UnrollFactor | WarmupCount | Mean     | Error   | StdDev  | Ratio | RatioSD |
|-------------------------- |----------- |---------------- |--------------- |------------ |------------- |------------ |---------:|--------:|--------:|------:|--------:|
| RangeFinder_Construction  | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           |  11.28 s |      NA | 0.000 s |  0.05 |    0.00 |
| IntervalTree_Construction | Job-HXLBIX | 1               | 1              | 1           | 1            | 1           | 218.22 s |      NA | 0.000 s |  1.00 |    0.00 |
|                           |            |                 |                |             |              |             |          |         |         |       |         |
| RangeFinder_Construction  | .NET 8.0   | Default         | Default        | Default     | 16           | Default     |  11.30 s | 0.010 s | 0.009 s |  0.05 |    0.00 |
| IntervalTree_Construction | .NET 8.0   | Default         | Default        | Default     | 16           | Default     | 207.41 s | 3.981 s | 5.034 s |  1.00 |    0.03 |
