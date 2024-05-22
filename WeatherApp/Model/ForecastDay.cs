using Newtonsoft.Json;

namespace WeatherApp.Model
{
    public class ForecastDay
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("day")]
        public Day Day { get; set; }
    }
}
