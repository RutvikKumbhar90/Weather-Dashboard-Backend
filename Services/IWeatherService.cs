using WeatherDashboardBackend.Models;

namespace WeatherDashboardBackend.Services
{
    public interface IWeatherService
    {
        Task<WeatherResponse?> GetCurrentWeatherAsync(string? city, double? latitude = null, double? longitude = null);
        Task<List<HourlyForecast>?> GetHourlyForecastAsync(string? city = null, double? latitude = null, double? longitude = null);

    }
}
