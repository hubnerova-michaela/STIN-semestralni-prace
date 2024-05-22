using Newtonsoft.Json;

namespace WeatherApp.Model
{
    public class Location
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("tz_id")]
        public string TimezoneId { get; set; }

        [JsonProperty("localtime_epoch")]
        public long LocaltimeEpoch { get; set; }

        [JsonProperty("localtime")]
        public string Localtime { get; set; }
    }
}
