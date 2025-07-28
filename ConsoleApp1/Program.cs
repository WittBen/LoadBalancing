using ConsoleApp1.Helpers;
using ConsoleApp1.LoadBalancerTypes;

public class Program
{
  public static async Task Main(string[] args)
  {
    // The main program loop for the menu
    while (true)
    {
      DisplayMenu();
      string choice = Console.ReadLine()?.ToLower(); // Reads user input

      switch (choice)
      {
        case "1":
          await DemonstrateRoundRobinAsync();
          break;
        case "2":
          await DemonstrateWeightedAsync();
          break;
        case "3":
          await DemonstrateAdaptiveAsync();
          break;
        case "q":
          Console.WriteLine("\nExiting program.");
          return; // Exits the application
        default:
          Console.WriteLine("\nInvalid input! Please choose one of the options.");
          Console.WriteLine("Press any key to return to the menu...");
          Console.ReadKey();
          break;
      }
    }
  }

  /// <summary>
  /// Displays the main menu in the console.
  /// </summary>
  private static void DisplayMenu()
  {
    Console.Clear();
    Console.WriteLine("=================================================");
    Console.WriteLine("    C# Load Balancer Demonstrations Menu");
    Console.WriteLine("=================================================");
    Console.WriteLine();
    Console.WriteLine("Select a demonstration:");
    Console.WriteLine("  1. Round-Robin Load Balancing");
    Console.WriteLine("  2. Weighted Load Balancing");
    Console.WriteLine("  3. Adaptive Load Balancing");
    Console.WriteLine();
    Console.WriteLine("  q. Exit Program");
    Console.WriteLine();
    Console.Write("Your choice: ");
  }

  // --- DEMONSTRATION 1: ROUND ROBIN ---
  private static async Task DemonstrateRoundRobinAsync()
  {
    Console.Clear();
    Console.WriteLine("=================================================");
    Console.WriteLine("  DEMONSTRATION 1: Round-Robin Load Balancing  ");
    Console.WriteLine("=================================================");
    Console.WriteLine("Principle: Requests are distributed strictly in turn to servers A, B, and C.\n");

    var serverPool = new[] { "http://server-a", "http://server-b", "http://server-c" };
    var balancer = new RoundRobinLoadBalancer(serverPool);

    for (int i = 1; i <= 25; i++)
    {
      string target = "";
      Console.WriteLine($"--- Request #{i} ---");

      try
      {
        await balancer.ForwardRequestAsync(
            new HttpRequestMessage(HttpMethod.Get, "http://client.local/api/data"),
            t => target = t // Store the target as soon as it's selected
        );

        DrawHelper.DrawFlow(target, "Success!");
      }
      catch (Exception e)
      {
        DrawHelper.DrawFlow(target, $"ERROR: {e.Message}", true);
        Console.WriteLine("      -> LB automatically tries the next server...");
        // In a real implementation, a retry would happen here.
        // For simplicity, we only show the first error.
      }
      await Task.Delay(500); // Short pause for readability
    }
    Console.WriteLine("Press any key to continue to the next demo...");
    Console.ReadKey();
  }

  // --- DEMONSTRATION 2: WEIGHTED ---
  private static async Task DemonstrateWeightedAsync()
  {
    Console.Clear();
    Console.WriteLine("=====================================================");
    Console.WriteLine("  DEMONSTRATION 2: Weighted Load Balancing");
    Console.WriteLine("=====================================================");
    Console.WriteLine("Principle: Servers with a higher weight receive more requests. (Server A)");

    var serverPool = new List<(string, int)>
    {
      ("http://server-a", 5), // 50%
      ("http://server-b", 1), // 10%
      ("http://server-c", 3)  // 30%
    };

    var balancer = new WeightedLoadBalancer(serverPool);
    var stats = serverPool.ToDictionary(s => s.Item1, s => 0);

    Console.WriteLine("Server weights: A=5, B=1, C=3");

    for (int i = 1; i <= 25; i++)
    {
      string target = "";

      Console.WriteLine($"--- Request #{i} ---");

      try
      {
        var response = await balancer.ForwardRequestAsync(
            new HttpRequestMessage(HttpMethod.Get, "http://client.local/api/data"),
            t => target = t
        );

        DrawHelper.DrawFlow(target, "Success!");

        stats[target]++;
      }
      catch (Exception e)
      {
        DrawHelper.DrawFlow(target, $"ERROR: {e.Message}", true);
      }
      await Task.Delay(200);
    }

    Console.WriteLine("--- Statistics after 25 requests ---");

    foreach (var stat in stats)
    {
      Console.WriteLine($"  {stat.Key}: {stat.Value} requests");
    }

    Console.WriteLine("Server A should have the most requests, Server B the fewest.");
    Console.WriteLine("Press any key to continue to the next demo...");
    Console.ReadKey();
  }

  // --- DEMONSTRATION 3: ADAPTIVE ---
  private static async Task DemonstrateAdaptiveAsync()
  {
    Console.Clear();
    Console.WriteLine("==================================================");
    Console.WriteLine("  DEMONSTRATION 3: Adaptive Load Balancing");
    Console.WriteLine("==================================================");
    Console.WriteLine("Principle: The LB measures latency & errors and selects the 'best' server.\n");

    var serverPool = new[] { "http://server-a", "http://server-b", "http://server-c" };
    var balancer = new AdaptiveLoadBalancer(serverPool);

    for (int i = 1; i <= 25; i++)
    {
      string target = "";
      Console.WriteLine($"\n--- Request #{i} ---");

      Console.WriteLine("State before the decision:");
      DrawHelper.DrawServerMetrics(balancer.ServerStats);

      try
      {
        await balancer.ForwardRequestAsync(
            new HttpRequestMessage(HttpMethod.Get, "http://client.local/api/data"),
            t => target = t
        );

        DrawHelper.DrawFlow(target, "Success!", false, balancer.ServerStats);
      }
      catch (Exception e)
      {
        DrawHelper.DrawFlow(target, $"ERROR: {e.Message}", true, balancer.ServerStats);
      }

      await Task.Delay(1000); // Longer pause to read the metrics
    }
    Console.WriteLine("Observation: The LB learns to avoid the slow server B and the faulty server C.");
    Console.WriteLine("Press any key to exit the program...");
    Console.ReadKey();
  }
}