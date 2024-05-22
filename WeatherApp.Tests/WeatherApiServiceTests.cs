using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using WeatherApp.Configuration;
using WeatherApp.Model;
using WeatherApp.Services;
using Xunit;

public class WeatherApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly WeatherApiService _weatherApiService;
    private readonly IOptions<WeatherApiConfiguration> _mockConfig;

    public WeatherApiServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var config = new WeatherApiConfiguration
        {
            BaseUrl = "http://mock-api-url.com"
        };
        _mockConfig = Options.Create(config);

        // Setup HttpClient using the mock message handler
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_mockConfig.Value.BaseUrl)
        };

        // Use the httpClient in the service constructor
        _weatherApiService = new WeatherApiService(_mockConfig, httpClient);
    }

    [Fact]
    public async Task GetWeatherAsync_ValidLocation_ReturnsWeatherData()
    {
        // Arrange
        var location = "Seattle";
        var expectedWeatherData = new CurrentWeather
        {
            Location = new Location { Name = location },
            Current = new WeatherData
            {
                LastUpdatedEpoch = 1625242800,
                LastUpdated = "2021-07-02 14:00",
                TempC = 22.0,
                TempF = 71.6,
                IsDay = 1,
                Condition = new Condition { Text = "Sunny", Icon = "//cdn.weatherapi.com/weather/64x64/day/113.png", Code = 1000 },
                WindMph = 5.6,
                WindKph = 9.0,
                WindDegree = 230,
                WindDir = "SW",
                PressureMb = 1015.0,
                PressureIn = 30.4,
                PrecipMm = 0.0,
                PrecipIn = 0.0,
                Humidity = 65,
                Cloud = 10,
                FeelslikeC = 24.0,
                FeelslikeF = 75.2,
                VisKm = 10.0,
                VisMiles = 6.0,
                UV = 5.0,
                GustMph = 7.2,
                GustKph = 11.5,
                AirQuality = new AirQuality
                {
                    CO = 0.4,
                    NO2 = 0.01,
                    O3 = 0.02,
                    SO2 = 0.005,
                    PM25 = 10,
                    PM10 = 20,
                    USEPAIndex = 1,
                    GBDEFRAIndex = 1
                }
            }
        };

        var jsonResponse = JsonConvert.SerializeObject(expectedWeatherData);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _weatherApiService.GetWeatherAsync(location);

        // Debugging: Log the result to inspect the deserialized object
        Console.WriteLine($"Location: {result?.Location?.Name}");
        Console.WriteLine($"TempC: {result?.Current?.TempC}");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(location, result.Location.Name);
        Assert.Equal(22.0, result.Current.TempC);
        Assert.Equal(71.6, result.Current.TempF);
        Assert.Equal(1, result.Current.IsDay);
        Assert.Equal("Sunny", result.Current.Condition.Text);
        Assert.Equal(5.6, result.Current.WindMph);
        Assert.Equal(9.0, result.Current.WindKph);
        Assert.Equal(230, result.Current.WindDegree);
        Assert.Equal("SW", result.Current.WindDir);
        Assert.Equal(1015.0, result.Current.PressureMb);
        Assert.Equal(30.4, result.Current.PressureIn);
        Assert.Equal(0.0, result.Current.PrecipMm);
        Assert.Equal(0.0, result.Current.PrecipIn);
        Assert.Equal(65, result.Current.Humidity);
        Assert.Equal(10, result.Current.Cloud);
        Assert.Equal(24.0, result.Current.FeelslikeC);
        Assert.Equal(75.2, result.Current.FeelslikeF);
        Assert.Equal(10.0, result.Current.VisKm);
        Assert.Equal(6.0, result.Current.VisMiles);
        Assert.Equal(5.0, result.Current.UV);
        Assert.Equal(7.2, result.Current.GustMph);
        Assert.Equal(11.5, result.Current.GustKph);
        Assert.NotNull(result.Current.AirQuality);
        Assert.Equal(0.4, result.Current.AirQuality.CO);
        Assert.Equal(0.01, result.Current.AirQuality.NO2);
        Assert.Equal(0.02, result.Current.AirQuality.O3);
        Assert.Equal(0.005, result.Current.AirQuality.SO2);
        Assert.Equal(10, result.Current.AirQuality.PM25);
        Assert.Equal(20, result.Current.AirQuality.PM10);
        Assert.Equal(1, result.Current.AirQuality.USEPAIndex);
        Assert.Equal(1, result.Current.AirQuality.GBDEFRAIndex);
    }

    [Fact]
    public async Task GetWeatherAsync_InvalidLocation_ReturnsNull()
    {
        // Arrange
        var location = "InvalidLocation";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _weatherApiService.GetWeatherAsync(location);

        // Assert
        Assert.Null(result);
    }
}
