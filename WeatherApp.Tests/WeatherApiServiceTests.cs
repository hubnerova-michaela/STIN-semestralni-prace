using Xunit;
using Moq;
using WeatherApp.Services;
using WeatherApp.Model;
using System.Threading.Tasks;
using FluentAssertions;

public class WeatherApiServiceTests
{
    [Fact]
    public async Task GetWeatherAsync_ValidCity_ReturnsWeatherData()
    {
        //// Arrange
        //var mockWeatherApiService = new Mock<IWeatherApiService>();
        //var expectedWeather = new CurrentWeather
        //{
        //    Current = new WeatherData { TempC = 25.0, Condition = new Condition { Text = "Sunny", Icon = "//cdn.weatherapi.com/weather/64x64/day/113.png" } },
        //    Location = new Location { Name = "London" }
        //};

        //mockWeatherApiService.Setup(service => service.GetWeatherAsync("London")).ReturnsAsync(expectedWeather);

        //// Act
        //var result = await mockWeatherApiService.Object.GetWeatherAsync("London");

        //// Assert
        //result.Should().BeEquivalentTo(expectedWeather);
    }

    [Fact]
    public async Task GetHistoricalWeatherForPastWeekAsync_ValidCity_ReturnsHistoricalData()
    {
        //// Arrange
        //var mockWeatherApiService = new Mock<IWeatherApiService>();
        //var expectedHistory = new List<HistoricalWeather>
        //{
        //    new HistoricalWeather
        //    {
        //        Location = new Location { Name = "London" },
        //        Forecast = new Forecast
        //        {
        //            ForecastDays = new List<ForecastDay>
        //            {
        //                new ForecastDay
        //                {
        //                    Date = DateTime.Today.AddDays(-1),
        //                    Day = new Day
        //                    {
        //                        MaxTempC = 20.0,
        //                        MinTempC = 10.0,
        //                        AvgTempC = 15.0,
        //                        Condition = new Condition { Text = "Cloudy", Icon = "//cdn.weatherapi.com/weather/64x64/day/119.png" }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //};

        //mockWeatherApiService.Setup(service => service.GetHistoricalWeatherForPastWeekAsync("London")).ReturnsAsync(expectedHistory);

        //// Act
        //var result = await mockWeatherApiService.Object.GetHistoricalWeatherForPastWeekAsync("London");

        //// Assert
        //result.Should().BeEquivalentTo(expectedHistory);
    }
}

