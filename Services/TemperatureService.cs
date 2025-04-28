using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WeatherDashboardBackend.Models;

public class TemperatureService : ITemperatureService
{
    private readonly HttpClient _httpClient;

    public TemperatureService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<TemperatureData>> GetTemperatureDataAsync(double latitude, double longitude)
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&daily=temperature_2m_max,temperature_2m_min&timezone=auto";
        var response = await _httpClient.GetStringAsync(url);

        dynamic data = JsonConvert.DeserializeObject(response);

        var temperatureData = new List<TemperatureData>();

        for (int i = 0; i < data.daily.time.Count; i++)
        {
            var date = DateTime.Parse(data.daily.time[i].ToString());
            temperatureData.Add(new TemperatureData
            {
                Day = date.DayOfWeek.ToString(),
                MinTemp = data.daily.temperature_2m_min[i],
                MaxTemp = data.daily.temperature_2m_max[i]
            });
        }

        return temperatureData;
    }

}
