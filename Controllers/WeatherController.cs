using Microsoft.AspNetCore.Mvc;
using WeatherDashboardBackend.Models;
using WeatherDashboardBackend.Services;
using System.Threading.Tasks;

namespace WeatherDashboardBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;

        public WeatherController(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        // Get current weather for a city or coordinates
        [HttpGet("current")]
        public async Task<ActionResult<WeatherResponse>> GetCurrentWeather([FromQuery] string? city, [FromQuery] double? latitude, [FromQuery] double? longitude)
        {
            if (string.IsNullOrWhiteSpace(city) && !(latitude.HasValue && longitude.HasValue))
            {
                return BadRequest("Either city name or coordinates are required.");
            }

            var weather = await _weatherService.GetCurrentWeatherAsync(city, latitude, longitude);

            if (weather == null)
            {
                return NotFound("Weather data not found.");
            }

            return Ok(weather);
        }

        // Get hourly forecast for a city
        [HttpGet("hourly")]
        public async Task<IActionResult> GetHourlyForecast([FromQuery] string? city, [FromQuery] double? latitude, [FromQuery] double? longitude)
        {
            if (string.IsNullOrWhiteSpace(city) && !(latitude.HasValue && longitude.HasValue))
            {
                return BadRequest("Either city name or coordinates are required.");
            }

            var hourlyData = await _weatherService.GetHourlyForecastAsync(city, latitude, longitude);

            if (hourlyData == null)
            {
                return NotFound("Unable to fetch hourly forecast.");
            }

            return Ok(hourlyData);
        }

    }
}
