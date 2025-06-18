using Avalonia;
using System;

namespace RangeFinder.Visualizer;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<RangeFinder.Visualizer.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}