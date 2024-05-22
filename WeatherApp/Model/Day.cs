using Newtonsoft.Json;

namespace WeatherApp.Model
{
    public class Day
    {
        [JsonProperty("maxtemp_c")]
        public double MaxTempC { get; set; }

        [JsonProperty("mintemp_c")]
        public double MinTempC { get; set; }

        [JsonProperty("avgtemp_c")]
        public double AvgTempC { get; set; }

        [JsonProperty("condition")]
        public Condition Condition { get; set; }
    }
}
