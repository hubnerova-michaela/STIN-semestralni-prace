using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WeatherApp.Data;
using WeatherApp.Model;
using WeatherApp.Services;

public class IndexModel : PageModel
{
    private readonly IWeatherApiService _weatherApiService;
    private readonly ApplicationDbContext _dbContext;

    public IndexModel(IWeatherApiService weatherApiService, ApplicationDbContext dbContext)
    {
        _weatherApiService = weatherApiService;
        _dbContext = dbContext;
    }

    [BindProperty]
    public CurrentWeather CurrentWeather { get; set; }
    public CurrentWeather CurrentLocationWeather { get; set; }
    public string CurrentLocation { get; set; }
    [BindProperty(SupportsGet = true)]
    public bool IsPremium { get; set; }
    public List<HistoricalWeather> HistoricalWeatherList { get; set; }

    public async Task OnGetAsync()
    {
        CurrentLocationWeather = await _weatherApiService.GetWeatherAsync("Liberec");
        CurrentLocation = "Liberec";
        if (User.Identity.IsAuthenticated)
        {
            //var user = await _userManager.GetUserAsync(User);
            var userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            if (userId != null)
            {
                var user = await _dbContext.FindAsync<ApplicationUser>(userId);
                if (user?.IsPremium == true)
                {
                    IsPremium = true;
                }
                else IsPremium = false;
            }
        }
    }


    public async Task<IActionResult> OnPostAsync(string city)
    {
        CurrentLocationWeather = await _weatherApiService.GetWeatherAsync("Liberec");
        CurrentLocation = "Liberec";
        if (!string.IsNullOrWhiteSpace(city))
        {
            CurrentWeather = await _weatherApiService.GetWeatherAsync(city);

            if (IsPremium)
            {
                HistoricalWeatherList = await _weatherApiService.GetHistoricalWeatherForPastWeekAsync(city);
            }
        }

        return Page();
    }
}