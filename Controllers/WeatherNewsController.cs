using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class WeatherNewsController : ControllerBase
{
    private readonly IWeatherNewsService _weatherNewsService;

    public WeatherNewsController(IWeatherNewsService weatherNewsService)
    {
        _weatherNewsService = weatherNewsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeatherNews()
    {
        var news = await _weatherNewsService.GetWeatherNewsAsync();
        return Ok(news);
    }
}