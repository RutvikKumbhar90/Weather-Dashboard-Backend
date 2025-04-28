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

        // Get current weather for a city
        [HttpGet("current")]
        public async Task<ActionResult<WeatherResponse>> GetCurrentWeather([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City name is required.");
            }

            var weather = await _weatherService.GetCurrentWeatherAsync(city);

            if (weather == null)
            {
                return NotFound("Weather data not found.");
            }

            return Ok(weather);
        }

        // Get hourly forecast for a city
        [HttpGet("hourly")]
        public async Task<IActionResult> GetHourlyForecast([FromQuery] string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("City name is required.");
            }

            var hourlyData = await _weatherService.GetHourlyForecastAsync(city);

            if (hourlyData == null)
            {
                return NotFound("Unable to fetch hourly forecast.");
            }

            return Ok(hourlyData);
        }

        // Get daily forecast for a city
    }
}
