namespace WeatherApp.Model
{
    public class WeatherHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string City { get; set; }
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }
        public string Condition { get; set; }
        public ApplicationUser User { get; set; }
    }

}
