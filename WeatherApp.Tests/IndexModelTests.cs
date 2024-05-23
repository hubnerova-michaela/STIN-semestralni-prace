using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using WeatherApp.Data;
using WeatherApp.Model;
using WeatherApp.Services;
using Xunit;

public class IndexModelTests
{
    private readonly Mock<IWeatherApiService> _mockWeatherApiService;

    public IndexModelTests()
    {
        _mockWeatherApiService = new Mock<IWeatherApiService>();
    }

    private ApplicationDbContext GetInMemoryDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private void SetUserClaims(IndexModel pageModel, string userId, bool isAuthenticated)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        var identity = new ClaimsIdentity(claims, isAuthenticated ? "mock" : "");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task OnGetAsync_PopulatesCurrentLocationWeather()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnGetAsync_PopulatesCurrentLocationWeather");
        var pageModel = new IndexModel(_mockWeatherApiService.Object, dbContext);

        var locationWeather = new CurrentWeather
        {
            Location = new Location { Name = "Liberec" },
            Current = new WeatherData { TempC = 10.0 }
        };

        _mockWeatherApiService.Setup(service => service.GetWeatherAsync("Liberec"))
            .ReturnsAsync(locationWeather);

        SetUserClaims(pageModel, "user1", true);

        var user = new ApplicationUser { Id = "user1", IsPremium = true };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        // Act
        await pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(pageModel.CurrentLocationWeather);
        Assert.Equal("Liberec", pageModel.CurrentLocationWeather.Location.Name);
        Assert.Equal(10.0, pageModel.CurrentLocationWeather.Current.TempC);
        Assert.Equal("Liberec", pageModel.CurrentLocation);
        Assert.True(pageModel.IsPremium);
    }

    [Fact]
    public async Task OnPostAsync_PopulatesCurrentWeatherAndHistoricalWeather_ForPremiumUser()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostAsync_PopulatesCurrentWeatherAndHistoricalWeather_ForPremiumUser");
        var pageModel = new IndexModel(_mockWeatherApiService.Object, dbContext);

        var city = "Prague";
        var currentWeather = new CurrentWeather
        {
            Location = new Location { Name = city },
            Current = new WeatherData { TempC = 12.0 }
        };

        var historicalWeather = new HistoricalWeather
        {
            Location = new Location { Name = city },
            Forecast = new Forecast
            {
                ForecastDays = new[]
                {
                    new ForecastDay { Date = "2023-01-01", Day = new Day { AvgTempC = 5.0 } },
                    new ForecastDay { Date = "2023-01-02", Day = new Day { AvgTempC = 6.0 } }
                }
            }
        };

        _mockWeatherApiService.Setup(service => service.GetWeatherAsync(city))
            .ReturnsAsync(currentWeather);

        _mockWeatherApiService.Setup(service => service.GetHistoricalWeatherForPastWeekAsync(city))
            .ReturnsAsync(new List<HistoricalWeather> { historicalWeather });

        pageModel.IsPremium = true;

        var currentLocationWeather = new CurrentWeather
        {
            Location = new Location { Name = "Liberec" },
            Current = new WeatherData { TempC = 10.0 }
        };

        _mockWeatherApiService.Setup(service => service.GetWeatherAsync("Liberec"))
            .ReturnsAsync(currentLocationWeather);

        // Act
        var result = await pageModel.OnPostAsync(city);

        // Assert
        Assert.Equal("Liberec", pageModel.CurrentLocation);
        Assert.Equal(currentLocationWeather, pageModel.CurrentLocationWeather);
        Assert.NotNull(pageModel.CurrentWeather);
        Assert.Equal(city, pageModel.CurrentWeather.Location.Name);
        Assert.Equal(12.0, pageModel.CurrentWeather.Current.TempC);
        Assert.NotNull(pageModel.HistoricalWeatherList);
        Assert.Single(pageModel.HistoricalWeatherList);
        Assert.Equal(2, pageModel.HistoricalWeatherList[0].Forecast.ForecastDays.Length);
        Assert.Equal(5.0, pageModel.HistoricalWeatherList[0].Forecast.ForecastDays[0].Day.AvgTempC);
        Assert.Equal(6.0, pageModel.HistoricalWeatherList[0].Forecast.ForecastDays[1].Day.AvgTempC);
    }

    [Fact]
    public async Task OnPostAsync_PopulatesOnlyCurrentWeather_ForNonPremiumUser()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostAsync_PopulatesOnlyCurrentWeather_ForNonPremiumUser");
        var pageModel = new IndexModel(_mockWeatherApiService.Object, dbContext);

        var city = "Prague";
        var currentWeather = new CurrentWeather
        {
            Location = new Location { Name = city },
            Current = new WeatherData { TempC = 12.0 }
        };

        _mockWeatherApiService.Setup(service => service.GetWeatherAsync(city))
            .ReturnsAsync(currentWeather);

        pageModel.IsPremium = false;

        var currentLocationWeather = new CurrentWeather
        {
            Location = new Location { Name = "Liberec" },
            Current = new WeatherData { TempC = 10.0 }
        };

        _mockWeatherApiService.Setup(service => service.GetWeatherAsync("Liberec"))
            .ReturnsAsync(currentLocationWeather);

        // Act
        var result = await pageModel.OnPostAsync(city);

        // Assert
        Assert.Equal("Liberec", pageModel.CurrentLocation);
        Assert.Equal(currentLocationWeather, pageModel.CurrentLocationWeather);
        Assert.NotNull(pageModel.CurrentWeather);
        Assert.Equal(city, pageModel.CurrentWeather.Location.Name);
        Assert.Equal(12.0, pageModel.CurrentWeather.Current.TempC);
        Assert.Null(pageModel.HistoricalWeatherList);
    }

    [Fact]
    public async Task OnPostAsync_WithEmptyCity_DoesNotPopulateWeather()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostAsync_WithEmptyCity_DoesNotPopulateWeather");
        var pageModel = new IndexModel(_mockWeatherApiService.Object, dbContext);

        string city = string.Empty;

        var currentLocationWeather = new CurrentWeather
        {
            Location = new Location { Name = "Liberec" },
            Current = new WeatherData { TempC = 10.0 }
        };

        _mockWeatherApiService.Setup(service => service.GetWeatherAsync("Liberec"))
            .ReturnsAsync(currentLocationWeather);

        // Act
        var result = await pageModel.OnPostAsync(city);

        // Assert
        Assert.Equal("Liberec", pageModel.CurrentLocation);
        Assert.Equal(currentLocationWeather, pageModel.CurrentLocationWeather);
        Assert.Null(pageModel.CurrentWeather);
        Assert.Null(pageModel.HistoricalWeatherList);
    }
}
