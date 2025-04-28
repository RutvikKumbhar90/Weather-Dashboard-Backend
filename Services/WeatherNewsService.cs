using System.Text.Json;
using WeatherDashboardBackend.Models;

public class WeatherNewsService : IWeatherNewsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WeatherNewsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["NewsApi:ApiKey"]!;
    }

    public async Task<WeatherNewsResponse> GetWeatherNewsAsync()
    {
        var requestUrl = $"https://newsapi.org/v2/everything?q=weather+forecast+climate+storm+rain+temperature&language=en&sortBy=publishedAt&pageSize=6&apiKey={_apiKey}";

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("User-Agent", "WeatherDashboardApp/1.0");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to fetch weather news. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var result = new WeatherNewsResponse
        {
            Status = root.GetProperty("status").GetString(),
            TotalResults = root.GetProperty("totalResults").GetInt32(),
            Articles = root.GetProperty("articles").EnumerateArray().Select(article => new WeatherArticle
            {
                Name = article.GetProperty("source").GetProperty("name").GetString(),
                Author = article.TryGetProperty("author", out var authorProp) ? authorProp.GetString() : null,
                Title = article.GetProperty("title").GetString(),
                Description = article.GetProperty("description").GetString(),
                Url = article.GetProperty("url").GetString(),
                UrlToImage = article.GetProperty("urlToImage").GetString(),
                PublishedAt = ConvertToIndianTime(article.GetProperty("publishedAt").GetDateTime())
            }).ToList()
        };

        return result;
    }

    private string ConvertToIndianTime(DateTime utcTime)
    {
        var indianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        var indianTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, indianTimeZone);
        return indianTime.ToString("yyyy-MM-dd hh:mm tt"); // e.g., "2025-04-24 03:45 PM"
    }
}
