using System.Threading.Tasks;
using WeatherDashboardBackend.Models;

public interface IWeatherNewsService
{
    Task<WeatherNewsResponse> GetWeatherNewsAsync();
}
