using ConsoleApp1.Server;

namespace ConsoleApp1.LoadBalancerTypes;

public class RoundRobinLoadBalancer
{
  private readonly string[] _serverUrls;
  private int _currentIndex = 0;
  private readonly object _indexLock = new object();

  public RoundRobinLoadBalancer(string[] serverUrls)
  {
    _serverUrls = serverUrls ?? throw new ArgumentNullException(nameof(serverUrls));
  }

  public async Task<HttpResponseMessage> ForwardRequestAsync(HttpRequestMessage clientRequest, Action<string> onForward)
  {
    Exception lastException = null;

    for (int attempt = 0; attempt < _serverUrls.Length; attempt++)
    {
      string targetServerUrl;

      lock (_indexLock)
      {
        targetServerUrl = _serverUrls[_currentIndex];
        _currentIndex = (_currentIndex + 1) % _serverUrls.Length;
      }

      onForward?.Invoke(targetServerUrl);

      var forwardRequest = new HttpRequestMessage(clientRequest.Method, targetServerUrl + clientRequest.RequestUri.PathAndQuery);
     
      try
      {
        var response = await MockBackendServer.HandleRequest(forwardRequest);
        if (response.IsSuccessStatusCode) return response;
        lastException = new HttpRequestException($"Server {targetServerUrl} responded with status {(int)response.StatusCode}");
        break;
      }
      catch (Exception ex)
      {
        lastException = ex;
      }
    }

    throw lastException ?? new Exception("Unknown error in the load balancer");
  }
}