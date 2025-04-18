using WeatherDashboardBackend.Models;

namespace WeatherDashboardBackend.Services
{
    public interface IWeatherService
    {
        Task<WeatherResponse?> GetCurrentWeatherAsync(string city);
        Task<List<HourlyForecast>?> GetHourlyForecastAsync(string city);


    }
}
