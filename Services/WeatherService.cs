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

                // Get timezone based on country code
                string timeZoneId = GetTimeZoneIdByCountryCode(countryCode);
                TimeZoneInfo userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                // Convert time to local
                long sunriseUnix = sys.GetProperty("sunrise").GetInt64();
                long sunsetUnix = sys.GetProperty("sunset").GetInt64();

                DateTime currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);
                DateTime sunriseTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(sunriseUnix).UtcDateTime, userTimeZone);
                DateTime sunsetTime = TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(sunsetUnix).UtcDateTime, userTimeZone);

                int uvIndex = await GetUvIndexAsync(lat, lon);
                // Ensure the description is not null by using a null-coalescing operator
                string suggestion = GetFriendlyWeatherSuggestion(weather.GetProperty("description").GetString() ?? "Unknown", precipitation, uvIndex);

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
                    Time = currentTime.ToString("hh:mm tt", CultureInfo.InvariantCulture),
                    City = city,
                    Country = country,
                    WeekDay = currentTime.ToString("dddd", CultureInfo.InvariantCulture),
                    Latitude = lat,
                    Longitude = lon,
                    Sunrise = sunriseTime.ToString("hh:mm tt"),
                    Sunset = sunsetTime.ToString("hh:mm tt"),
                    FriendlySuggestion = suggestion
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

                // Get the timezone offset (in seconds) for the city
                int timezoneOffset = 0;
                if (forecastData.TryGetProperty("city", out var cityData) &&
                    cityData.TryGetProperty("timezone", out var timezoneElement))
                {
                    timezoneOffset = timezoneElement.GetInt32();
                }

                foreach (var item in forecastList.EnumerateArray().Take(8)) // ~24 hours (3-hour intervals)
                {
                    var main = item.GetProperty("main");
                    var weather = item.GetProperty("weather")[0];
                    var utcTime = item.GetProperty("dt").GetInt64(); // Unix timestamp in UTC

                    // Convert UTC timestamp to local time using the offset
                    var localTime = DateTimeOffset.FromUnixTimeSeconds(utcTime)
                        .ToOffset(TimeSpan.FromSeconds(timezoneOffset))
                        .DateTime;

                    var formattedTime = localTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);

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

        // Helper: Get Windows timezone ID by country code
        private string GetTimeZoneIdByCountryCode(string countryCode)
        {
            return countryCode.ToUpper() switch
            {
                "IN" => "India Standard Time",
                "AE" => "Arabian Standard Time",
                "US" => "Eastern Standard Time", // or dynamic based on city
                "GB" => "GMT Standard Time",
                "JP" => "Tokyo Standard Time",
                "AU" => "AUS Eastern Standard Time",
                "FR" => "Romance Standard Time",
                "DE" => "W. Europe Standard Time",
                "CN" => "China Standard Time",
                "RU" => "Russian Standard Time",
                _ => "UTC"
            };
        }

        // Helper: Get a friendly suggestion based on weather conditions
        private string GetFriendlyWeatherSuggestion(string weatherDescription, float precipitation, int uvIndex)
        {
            if (weatherDescription.Contains("rain"))
            {
                return "Don't forget to carry an umbrella! ☔🌧️";
            }

            if (weatherDescription.Contains("clear"))
            {
                return uvIndex > 5
                    ? "It's sunny! Don't forget sunscreen. 🌞🧴"
                    : "It's a clear day, enjoy the sunshine! ☀️😊";
            }

            if (weatherDescription.Contains("clouds"))
            {
                return "It's cloudy today, a light jacket might be a good idea. ☁️🧥";
            }

            if (precipitation > 0)
            {
                return "It might rain, carry an umbrella just in case! 🌧️☂️";
            }

            if (weatherDescription.Contains("snow"))
            {
                return "It's snowing! Make sure to wear warm clothes. ❄️🧣🧤";
            }

            return "Stay comfortable and enjoy your day! 🌤️😌";

        }
    }
}
