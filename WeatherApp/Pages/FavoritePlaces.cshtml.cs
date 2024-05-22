using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Model;
using WeatherApp.Services;

namespace WeatherApp.Pages
{
    public class FavoritePlacesModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWeatherApiService _weatherApiService;

        public FavoritePlacesModel(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IWeatherApiService weatherApiService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _weatherApiService = weatherApiService;
        }

        public List<FavoritePlace> FavoritePlaces { get; set; }
        public Dictionary<string, Forecast> FavoritePlacesForecasts { get; set; } = new Dictionary<string, Forecast>();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsPremium)
            {
                return RedirectToPage("/Index");
            }

            FavoritePlaces = await _dbContext.FavoritePlaces.Where(fp => fp.UserId == user.Id).ToListAsync();

            foreach (var place in FavoritePlaces)
            {
                var forecast = await _weatherApiService.GetForecastAsync(place.City);
                FavoritePlacesForecasts[place.City] = forecast.Forecast;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var favoritePlace = await _dbContext.FavoritePlaces.FindAsync(id);
            if (favoritePlace == null)
            {
                return NotFound();
            }

            _dbContext.FavoritePlaces.Remove(favoritePlace);
            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveFavoriteAsync(string city)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsPremium)
            {
                return Unauthorized();
            }

            // Check if the city is already a favorite place for the user
            var existingFavorite = await _dbContext.FavoritePlaces
                .FirstOrDefaultAsync(fp => fp.City == city && fp.UserId == user.Id);

            if (existingFavorite != null)
            {
                return RedirectToPage();
            }

            var favoritePlace = new FavoritePlace
            {
                City = city,
                UserId = user.Id
            };

            await _dbContext.FavoritePlaces.AddAsync(favoritePlace);
            await _dbContext.SaveChangesAsync();

            return RedirectToPage();
        }
    }


}
