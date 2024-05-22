namespace WeatherApp.Model
{
    using Newtonsoft.Json;

    public class HistoricalWeather
    {
        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("forecast")]
        public Forecast Forecast { get; set; }
    }
}
