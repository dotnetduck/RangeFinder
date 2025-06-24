using ConsoleAppFramework;

var app = ConsoleApp.Create();
app.Add("run-single", BenchmarkCommands.RunSingle);
app.Add("run-suite", BenchmarkCommands.RunSuite);
app.Add("debug-characteristics", BenchmarkCommands.DebugCharacteristics);
app.Run(args);
