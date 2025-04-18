using System.Globalization;
using System.Text.Json;
using WeatherDashboardBackend.Models;

namespace WeatherDashboardBackend.Services
{
    public class WeatherService : IWeatherService
    {

        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenWeather:ApiKey"];
        }

        public async Task<WeatherResponse?> GetCurrentWeatherAsync(string city)
        {
            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";
                var response = await _httpClient.GetStringAsync(url);
                var weatherData = JsonDocument.Parse(response).RootElement;

                var main = weatherData.GetProperty("main");
                var weather = weatherData.GetProperty("weather")[0];
                var wind = weatherData.GetProperty("wind");
                var coord = weatherData.GetProperty("coord");
                var sys = weatherData.GetProperty("sys");

                float precipitation = 0;
                if (weatherData.TryGetProperty("rain", out var rain) &&
                    rain.TryGetProperty("1h", out var rain1h))
                {
                    precipitation = rain1h.GetSingle();
                }

                float feelsLike = main.TryGetProperty("feels_like", out var feelsLikeEl) ? feelsLikeEl.GetSingle() : 0;
                float dewPoint = main.TryGetProperty("dew_point", out var dewEl) ? dewEl.GetSingle() : 0;
                int visibility = weatherData.TryGetProperty("visibility", out var visEl) ? visEl.GetInt32() : 0;
                int cloudCover = 0;
                if (weatherData.TryGetProperty("clouds", out var cloudEl) &&
                    cloudEl.TryGetProperty("all", out var cloudAll))
                {
                    cloudCover = cloudAll.ValueKind == JsonValueKind.Number
                        ? (int)cloudAll.GetDouble()
                        : 0;
                }

                int windDirection = wind.TryGetProperty("deg", out var windDeg) ? windDeg.GetInt32() : 0;

                float lat = coord.GetProperty("lat").GetSingle();
                float lon = coord.GetProperty("lon").GetSingle();

                string countryCode = sys.TryGetProperty("country", out var countryEl) ? countryEl.GetString() ?? "" : "";
                string country = new RegionInfo(countryCode).EnglishName;

                // Convert sunrise and sunset from Unix time to India time
                long sunriseUnix = sys.GetProperty("sunrise").GetInt64();
                long sunsetUnix = sys.GetProperty("sunset").GetInt64();
                var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                var sunriseTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(sunriseUnix).UtcDateTime, indiaTimeZone);
                var sunsetTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(sunsetUnix).UtcDateTime, indiaTimeZone);

                int uvIndex = await GetUvIndexAsync(lat, lon);

                var indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

                return new WeatherResponse
                {
                    Temperature = main.GetProperty("temp").GetSingle(),
                    FeelsLike = feelsLike,
                    Humidity = main.GetProperty("humidity").GetInt32(),
                    Pressure = main.GetProperty("pressure").GetInt32(),
                    WindSpeed = wind.GetProperty("speed").GetSingle(),
                    WindDirection = windDirection,
                    CloudCover = cloudCover,
                    Visibility = visibility,
                    DewPoint = dewPoint,
                    UvIndex = uvIndex,
                    Precipitation = precipitation,
                    WeatherDescription = weather.GetProperty("description").GetString(),
                    WeatherIcon = weather.GetProperty("icon").GetString(),
                    Time = indiaTime.ToString("hh:mm tt"),
                    City = city,
                    Country = country,
                    WeekDay = indiaTime.ToString("dddd", CultureInfo.InvariantCulture),
                    Latitude = lat,
                    Longitude = lon,
                    Sunrise = sunriseTime.ToString("hh:mm tt"),
                    Sunset = sunsetTime.ToString("hh:mm tt")
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<int> GetUvIndexAsync(float lat, float lon)
        {
            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/uvi?lat={lat}&lon={lon}&appid={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var uvData = JsonDocument.Parse(response).RootElement;
                return (int)uvData.GetProperty("value").GetSingle();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<HourlyForecast>?> GetHourlyForecastAsync(string city)
        {
            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={_apiKey}&units=metric";
                var response = await _httpClient.GetStringAsync(url);
                var forecastData = JsonDocument.Parse(response).RootElement;

                var forecastList = forecastData.GetProperty("list");
                var result = new List<HourlyForecast>();

                foreach (var item in forecastList.EnumerateArray().Take(8)) // ~24 hours (3-hour intervals)
                {
                    var main = item.GetProperty("main");
                    var weather = item.GetProperty("weather")[0];
                    var time = item.GetProperty("dt_txt").GetString();

                    var formattedTime = DateTime.Parse(time ?? "").ToString("h:mm tt", CultureInfo.InvariantCulture);

                    result.Add(new HourlyForecast
                    {
                        Time = formattedTime,
                        Temperature = main.GetProperty("temp").GetSingle(),
                       
                    });
                }

                return result;
            }
            catch
            {
                return null;
            }
        }



    }
}
