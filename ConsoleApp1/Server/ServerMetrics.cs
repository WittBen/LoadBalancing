namespace ConsoleApp1.Server;

public class ServerMetrics
{
  public string Url;
  public double AvgLatencyMs;
  public int FailCount;
  public ServerMetrics(string url) 
  {
    Url = url; 
    AvgLatencyMs = 0; 
    FailCount = 0; 
  }
}