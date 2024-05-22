using Newtonsoft.Json;

namespace WeatherApp.Model
{
    public class LocationWeather
    {
        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("current")]
        public WeatherData Current { get; set; }
    }
}
