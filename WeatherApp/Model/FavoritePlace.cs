namespace WeatherApp.Model
{
    public class FavoritePlace
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string City { get; set; }
        public ApplicationUser User { get; set; }
    }

}
