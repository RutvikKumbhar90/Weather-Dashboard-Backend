namespace WeatherDashboardBackend.Models
{
    public class WeatherResponse
    {
        public float Temperature { get; set; }
        public float FeelsLike { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
        public float WindSpeed { get; set; }
        public int WindDirection { get; set; }
        public int CloudCover { get; set; }
        public int Visibility { get; set; }
        public string? WeatherDescription { get; set; }
        public string? WeatherIcon { get; set; }
        public float DewPoint { get; set; }
        public int UvIndex { get; set; }
        public float Precipitation { get; set; }
        public string? WeekDay { get; set; }
        public string? Time { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Sunrise { get; set; }
        public string? Sunset { get; set; }

        public float? Latitude { get; set; }
        public float? Longitude { get; set; }

        // Added for weather advice
        public string? FriendlySuggestion { get; set; }
    }
}
