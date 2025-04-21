namespace WeatherDashboardBackend.Models
{
    public class WeatherNewsResponse
    {
        public string? Status { get; set; }
        public int TotalResults { get; set; }
        public List<WeatherArticle>? Articles { get; set; }
    }
}
