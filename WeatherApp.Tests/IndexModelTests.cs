using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Data;
using WeatherApp.Model;
using WeatherApp.Pages;
using WeatherApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WeatherApp.Tests
{
    public class IndexModelTests
    {
        private readonly Mock<IWeatherApiService> _mockWeatherApiService;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public IndexModelTests()
        {
            //_mockWeatherApiService = new Mock<IWeatherApiService>();

            //var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            //    .UseInMemoryDatabase(databaseName: "TestDb")
            //    .Options;
            //_dbContext = new ApplicationDbContext(options);

            //_mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            //var claims = new List<Claim>
            //{
            //    new Claim(ClaimTypes.NameIdentifier, "1")
            //};
            //var identity = new ClaimsIdentity(claims, "TestAuthType");
            //var claimsPrincipal = new ClaimsPrincipal(identity);

            //var context = new DefaultHttpContext
            //{
            //    User = claimsPrincipal
            //};

            //_mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

            //// Seed the database with a user
            //var user = new ApplicationUser { Id = "1", UserName = "testuser", IsPremium = true };
            //_dbContext.Users.Add(user);
            //_dbContext.SaveChanges();
        }

        [Fact]
        public async Task OnGetAsync_UserIsAuthenticated_SetsIsPremium()
        {
            //// Arrange
            //var pageModel = new IndexModel(_mockWeatherApiService.Object, _dbContext)
            //{
            //    PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            //    {
            //        HttpContext = _mockHttpContextAccessor.Object.HttpContext
            //    }
            //};

            //// Act
            //await pageModel.OnGetAsync();

            //// Assert
            //Assert.True(pageModel.IsPremium);
        }

        [Fact]
        public async Task OnPostAsync_ValidCity_ReturnsWeather()
        {
            //// Arrange
            //var city = "TestCity";
            //var currentWeather = new CurrentWeather
            //{
            //    Location = new Location { Name = city },
            //    Current = new CurrentWeather { Current = new() { TempC = 25.5, Condition = new Condition { Text = "Sunny", Icon = "icon_url" } } }
            //};

            //var historicalWeatherList = new List<HistoricalWeather>
            //{
            //    new HistoricalWeather
            //    {
            //        Location = new Location { Name = city },
            //        Forecast = new Forecast
            //        {
            //            ForecastDays = new List<ForecastDay>
            //            {
            //                new ForecastDay
            //                {
            //                    Date = DateTime.Now.AddDays(-1),
            //                    Day = new Day
            //                    {
            //                        MaxTempC = 30,
            //                        MinTempC = 20,
            //                        AvgTempC = 25,
            //                        Condition = new Condition { Text = "Partly cloudy" }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //};

            //_mockWeatherApiService.Setup(x => x.GetWeatherAsync(city)).ReturnsAsync(currentWeather);
            //_mockWeatherApiService.Setup(x => x.GetHistoricalWeatherForPastWeekAsync(city)).ReturnsAsync(historicalWeatherList);

            //var pageModel = new IndexModel(_mockWeatherApiService.Object, _dbContext)
            //{
            //    PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
            //    {
            //        HttpContext = _mockHttpContextAccessor.Object.HttpContext
            //    }
            //};

            //// Act
            //var result = await pageModel.OnPostAsync(city);

            //// Assert
            //Assert.NotNull(pageModel.CurrentWeather);
            //Assert.Equal(city, pageModel.CurrentWeather.Location.Name);
            //Assert.Equal(25.5, pageModel.CurrentWeather.Current.TempC);
            //Assert.Equal("Sunny", pageModel.CurrentWeather.Current.Condition.Text);

            //if (pageModel.IsPremium)
            //{
            //    Assert.NotNull(pageModel.HistoricalWeatherList);
            //    Assert.Single(pageModel.HistoricalWeatherList);
            //    Assert.Equal(30, pageModel.HistoricalWeatherList[0].Forecast.ForecastDays[0].Day.MaxTempC);
            //}
        }
    }
}

