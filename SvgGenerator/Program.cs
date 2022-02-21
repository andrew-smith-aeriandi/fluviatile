using System;

namespace SvgGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        
        Console.WriteLine("Hello, World!");
    }
}
