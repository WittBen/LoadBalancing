using ConsoleApp1.Server;

namespace ConsoleApp1.LoadBalancerTypes;

public class AdaptiveLoadBalancer
{
  private readonly List<ServerMetrics> _servers;
  private readonly object _metricsLock = new object();

  public IReadOnlyList<ServerMetrics> ServerStats => _servers;

  public AdaptiveLoadBalancer(IEnumerable<string> serverUrls)
  {
    _servers = serverUrls.Select(url => new ServerMetrics(url)).ToList();
  }

  public async Task<HttpResponseMessage> ForwardRequestAsync(HttpRequestMessage clientRequest, Action<string> onForward)
  {
    ServerMetrics chosenServer;

    lock (_metricsLock)
    {
      chosenServer = _servers.Where(s => s.FailCount < 3)
          .OrderBy(s => s.AvgLatencyMs + (s.FailCount * 1000))
          .FirstOrDefault() ?? _servers.OrderBy(s => s.FailCount).First();
    }

    onForward?.Invoke(chosenServer.Url);

    var forwardRequest = new HttpRequestMessage(clientRequest.Method, chosenServer.Url + clientRequest.RequestUri.PathAndQuery);
    var stopwatch = new System.Diagnostics.Stopwatch();

    try
    {
      stopwatch.Start();

      var response = await MockBackendServer.HandleRequest(forwardRequest);

      stopwatch.Stop();

      lock (_metricsLock)
      {
        double latency = stopwatch.Elapsed.TotalMilliseconds;
        if (chosenServer.AvgLatencyMs == 0)
        {
          chosenServer.AvgLatencyMs = latency;
        }
        else
        {
          chosenServer.AvgLatencyMs = (chosenServer.AvgLatencyMs * 0.7) + (latency * 0.3); // Gleitender Durchschnitt
        }

        chosenServer.FailCount = 0;
      }

      return response;
    }
    catch (Exception ex)
    {
      stopwatch.Stop();

      lock (_metricsLock)
      {
        chosenServer.FailCount++;
        chosenServer.AvgLatencyMs = Math.Max(chosenServer.AvgLatencyMs, 5000);
      }

      throw;
    }
  }
}