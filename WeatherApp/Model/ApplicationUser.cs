using Microsoft.AspNetCore.Identity;

namespace WeatherApp.Model
{

    public class ApplicationUser : IdentityUser
    {
        public bool IsPremium { get; set; }  // For premium status
        public List<FavoritePlace> FavoritePlaces { get; set; }
    }
}
