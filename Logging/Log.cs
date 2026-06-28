using System;
using System.Collections.Generic;
using System.Text;

namespace KeyEngine.Logging;

public static class Log
{
    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public static void Warning(string message)
    {
        Console.WriteLine($"[WARN] {message}");
    }

    public static void Error(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }
}
