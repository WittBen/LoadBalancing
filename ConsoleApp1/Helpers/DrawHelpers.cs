using ConsoleApp1.Server;

namespace ConsoleApp1.Helpers;

public static class DrawHelper
{
  public static void DrawFlow(string target, string result, bool isError = false, IReadOnlyList<ServerMetrics> metrics = null)
  {
    var originalColor = Console.ForegroundColor;
    Console.Write("[ LB ] --Request--> ");

    string[] servers = { "http://server-a", "http://server-b", "http://server-c" };
    foreach (var server in servers)
    {
      if (server == target)
      {
        Console.ForegroundColor = isError ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"[{server.Split('-')[1].ToUpper()}]");
        Console.ForegroundColor = originalColor;
      }
      else
      {
        Console.Write($"[ {server.Split('-')[1].ToUpper()} ]");
      }
      Console.Write(" ");
    }

    Console.ForegroundColor = isError ? ConsoleColor.Red : ConsoleColor.Green;
    Console.WriteLine($"\n      -> RESULT: {result}");
    Console.ForegroundColor = originalColor;
  }

  public static void DrawServerMetrics(IReadOnlyList<ServerMetrics> metrics)
  {
    foreach (var s in metrics.OrderBy(m => m.Url))
    {
      Console.WriteLine($"  - [Server {s.Url.Split('-')[1].ToUpper()}] AvgLatency: {s.AvgLatencyMs,4:F0}ms, FailCount: {s.FailCount}");
    }

    Console.WriteLine();
  }
}