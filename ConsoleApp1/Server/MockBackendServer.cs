using System.Net;

namespace ConsoleApp1.Server;

public static class MockBackendServer
{
  private static int _requestCountServerC = 0;

  public static async Task<HttpResponseMessage> HandleRequest(HttpRequestMessage request)
  {
    string serverUrl = request.RequestUri.GetLeftPart(UriPartial.Authority);
    int latency = 100;
    string serverName = serverUrl.Split('-')[1].ToUpper();

    Console.Write($"      <- Request received at [Server {serverName}]. ");

    switch (serverUrl)
    {
      case "http://server-a":
        latency = 50;
        Console.WriteLine($"Responding in {latency}ms (fast).");
        break;
      case "http://server-b":
        latency = 400;
        Console.WriteLine($"Responding in {latency}ms (slow).");
        break;
      case "http://server-c":
        _requestCountServerC++;
        if (_requestCountServerC % 3 == 0)
        {
          Console.WriteLine("SIMULATING ERROR!");
          await Task.Delay(20);
          throw new TimeoutException($"Server C (unreliable) is overloaded.");
        }
        latency = 150;
        Console.WriteLine($"Responding in {latency}ms.");
        break;
    }

    await Task.Delay(latency);
    var response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
    return response;
  }
}