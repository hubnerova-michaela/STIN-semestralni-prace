using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WeatherApp.Configuration;
using WeatherApp.Model;

namespace WeatherApp.Services
{
    public class WeatherApiService : IWeatherApiService
    {
        public HttpClient _httpClient;
        public readonly IOptions<WeatherApiConfiguration> _configuration;

        public WeatherApiService(IOptions<WeatherApiConfiguration> configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient() { 
                BaseAddress = new Uri(_configuration.Value.BaseUrl)
            };
        }
        public async Task<CurrentWeather> GetWeatherAsync(string city)
        {
            var response = await _httpClient.GetAsync($"current.json?q={city}&key={_configuration.Value.ApiKey}");

            var responseMessage = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CurrentWeather>(responseMessage);

            return model;
        }
    }


    public interface IWeatherApiService
    {
        public Task<CurrentWeather> GetWeatherAsync(string city);

    }


}
