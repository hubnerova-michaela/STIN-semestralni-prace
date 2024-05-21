using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
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

        public async Task<HistoricalWeather> GetHistoricalWeatherAsync(string city, DateTime date)
        {
            string dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var response = await _httpClient.GetAsync($"history.json?q={city}&dt={dateString}&key={_configuration.Value.ApiKey}");
            var responseMessage = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<HistoricalWeather>(responseMessage);

            return model;
        }

        public async Task<List<HistoricalWeather>> GetHistoricalWeatherForPastWeekAsync(string city)
        {
            var historicalWeatherList = new List<HistoricalWeather>();
            for (int i = 1; i < 8; i++)
            {
                var date = DateTime.UtcNow.AddDays(-i);
                var historicalWeather = await GetHistoricalWeatherAsync(city, date);
                historicalWeatherList.Add(historicalWeather);
            }
            return historicalWeatherList;
        }

    }


    public interface IWeatherApiService
    {
        public Task<CurrentWeather> GetWeatherAsync(string city);
        public Task<HistoricalWeather> GetHistoricalWeatherAsync(string city, DateTime date);
        Task<List<HistoricalWeather>> GetHistoricalWeatherForPastWeekAsync(string city);

    }


}
