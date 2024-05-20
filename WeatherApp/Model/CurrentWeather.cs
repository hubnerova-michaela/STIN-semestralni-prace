using Newtonsoft.Json;

namespace WeatherApp.Model
{
    public class CurrentWeather
    {
        //[JsonProperty("location")]
        //public Location Location { get; set; }

        [JsonProperty("current")]
        public WeatherData Current { get; set; }
    }
}
