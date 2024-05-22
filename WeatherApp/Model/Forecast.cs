using Newtonsoft.Json;

namespace WeatherApp.Model
{
    public class Forecast
    {
        [JsonProperty("forecastday")]
        public ForecastDay[] ForecastDays { get; set; }
    }
}
