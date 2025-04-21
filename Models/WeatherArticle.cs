namespace WeatherDashboardBackend.Models
{
    public class WeatherArticle
    {
        public string? Name { get; set; }           // Source name
        public string? Author { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? UrlToImage { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
