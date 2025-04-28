using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherDashboardBackend.Models;
using WeatherDashboardBackend.Services;

[ApiController]
[Route("api/[controller]")]
public class TemperatureController : ControllerBase
{
    private readonly ITemperatureService _temperatureService;
    private readonly IWeatherService _weatherService;

    public TemperatureController(ITemperatureService temperatureService, IWeatherService weatherService)
    {
        _temperatureService = temperatureService;
        _weatherService = weatherService;
    }

    [HttpGet("{city}")]
    public async Task<ActionResult<List<TemperatureData>>> GetTemperatureData(string city)
    {
        // Step 1: Get latitude and longitude from WeatherService
        var weather = await _weatherService.GetCurrentWeatherAsync(city);

        if (weather == null)
        {
            return NotFound(new { message = "City not found." });
        }

        // Step 2: Get 7-day temperature data from TemperatureService using lat/lon
        var temperatureData = await _temperatureService.GetTemperatureDataAsync(
            weather.Latitude.Value, weather.Longitude.Value);

        return Ok(new { daily = temperatureData });
    }
}
