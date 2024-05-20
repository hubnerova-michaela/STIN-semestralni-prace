using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WeatherApp.Model;
using WeatherApp.Services;

namespace WeatherApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IWeatherApiService _weatherApiService;


        public IndexModel(ILogger<IndexModel> logger, IWeatherApiService weatherApiService)
        {
            _logger = logger;
            _weatherApiService = weatherApiService;
        }

        [BindProperty(SupportsGet = true)]
        public string City { get; set; }

        public CurrentWeather Weather { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(City))
            {
                Weather = await _weatherApiService.GetWeatherAsync(City);
            }
        }

        //public void OnPost()
        //{
        //    string city = Request.Form["city"];
        //    var weatherData = _weatherApiService.GetWeatherAsync(city).Result;
        //}
    }
}
