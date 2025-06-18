using Avalonia;
using System;

namespace RangeFinder.Visualize;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<RangeFinder.Visualize.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}