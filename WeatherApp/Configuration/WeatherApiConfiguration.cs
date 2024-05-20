using Microsoft.Extensions.Primitives;

namespace WeatherApp.Configuration
{
    public class WeatherApiConfiguration
    {


        public string ApiKey { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;



    }


}
