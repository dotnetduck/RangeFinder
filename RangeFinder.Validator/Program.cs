using ConsoleAppFramework;

namespace RangeFinder.Validator;

public class Program
{
    public static void Main(string[] args)
    {
        // Run with ConsoleAppFramework
        var app = ConsoleApp.Create();
        app.Add<Commands>();
        app.Run(args);
    }
}