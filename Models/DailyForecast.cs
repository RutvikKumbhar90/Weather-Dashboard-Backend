namespace WeatherDashboardBackend.Models
{
    public class DailyForecast
    {
        public string? Day { get; set; } 
        public float MinTemperature { get; set; }
        public float MaxTemperature { get; set; }
    }
}
