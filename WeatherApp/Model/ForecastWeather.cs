using Newtonsoft.Json;

namespace WeatherApp.Model
{
    public class ForecastWeather
    {
        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("current")]
        public WeatherData Current { get; set; }

        [JsonProperty("forecast")]
        public Forecast Forecast { get; set; }
    }
}
