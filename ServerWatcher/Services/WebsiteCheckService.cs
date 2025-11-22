using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServerWatcher.Services;

public class WebsiteCheckService
{
    private readonly HttpClient _httpClient;
    private const int TimeoutSeconds = 10;

    public WebsiteCheckService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
        };
    }

    public async Task<(bool Success, string Message)> CheckWebsiteAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return (false, "No URL configured");
        }

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return (true, $"OK ({(int)response.StatusCode})");
            }
            else
            {
                return (false, $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
            }
        }
        catch (TaskCanceledException)
        {
            return (false, "Request timed out");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }
}
