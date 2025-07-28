using ConsoleApp1.Server;

namespace ConsoleApp1.LoadBalancerTypes;

public class WeightedLoadBalancer
{
  private readonly List<(string Url, int Weight)> _servers;
  private readonly Random _rand = new Random();

  public WeightedLoadBalancer(List<(string, int)> serversWithWeights)
  {
    _servers = serversWithWeights ?? throw new ArgumentNullException(nameof(serversWithWeights));
  }

  public async Task<HttpResponseMessage> ForwardRequestAsync(HttpRequestMessage clientRequest, Action<string> onForward)
  {
    var availableServers = new List<(string Url, int Weight)>(_servers);

    Exception lastException = null;

    for (int attempt = 0; attempt < _servers.Count; attempt++)
    {
      int totalWeight = availableServers.Sum(s => s.Weight);
      
      if (totalWeight <= 0) 
      {
        break; 
      }

      int rnd = _rand.Next(totalWeight);
      string targetUrl = null;
      int cumulative = 0;

      foreach (var server in availableServers)
      {
        cumulative += server.Weight;
       
        if (rnd < cumulative)
        {
          targetUrl = server.Url;
          break;
        }
      }

      if (targetUrl == null) 
      {
        continue;
      }

      onForward?.Invoke(targetUrl);

      var forwardRequest = new HttpRequestMessage(clientRequest.Method, targetUrl + clientRequest.RequestUri.PathAndQuery);
      try
      {
        var response = await MockBackendServer.HandleRequest(forwardRequest);
        
        if (response.IsSuccessStatusCode) 
        {
          return response; 
        }

        lastException = new HttpRequestException($"Server {targetUrl} responded with status {(int)response.StatusCode}");

        break;
      }
      catch (Exception ex)
      {
        lastException = ex;
        availableServers.RemoveAll(s => s.Url == targetUrl);
      }
    }
    throw lastException ?? new Exception("No response from the weighted servers");
  }
}