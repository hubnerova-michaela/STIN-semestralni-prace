using System;
using System.Collections.Generic;
using System.Globalization;
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
            BaseUrl = "http://mock-api-url.com",
            ApiKey = "fake-api-key"
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
                Condition = new Condition { Text = "Sunny" },
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

        // Assert
        Assert.NotNull(result);
        Assert.Equal(location, result.Location.Name);
        Assert.Equal(22.0, result.Current.TempC);
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

    [Fact]
    public async Task GetHistoricalWeatherAsync_ValidLocationAndDate_ReturnsHistoricalWeather()
    {
        // Arrange
        var city = "London";
        var date = DateTime.UtcNow.AddDays(-1);
        var expectedResponse = new HistoricalWeather
        {
            Location = new Location { Name = city },
            Forecast = new Forecast { ForecastDays = new List<ForecastDay> { new ForecastDay { Date = date.ToString("yyyy-MM-dd") } }.ToArray() }
        };
        var jsonResponse = JsonConvert.SerializeObject(expectedResponse);
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
        var result = await _weatherApiService.GetHistoricalWeatherAsync(city, date);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(city, result.Location.Name);
        Assert.Equal(date.ToString("yyyy-MM-dd"), result.Forecast.ForecastDays[0].Date);
    }

    [Fact]
    public async Task GetHistoricalWeatherAsync_InvalidLocation_ReturnsNull()
    {
        // Arrange
        var city = "InvalidLocation";
        var date = DateTime.UtcNow.AddDays(-1);
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _weatherApiService.GetHistoricalWeatherAsync(city, date);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetHistoricalWeatherAsync_Non200StatusCode_ReturnsNull()
    {
        // Arrange
        var city = "London";
        var date = DateTime.UtcNow.AddDays(-1);
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _weatherApiService.GetHistoricalWeatherAsync(city, date);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetHistoricalWeatherForPastWeekAsync_ValidLocation_ReturnsWeatherList()
    {
        // Arrange
        var city = "Paris";
        var historicalWeatherList = new List<HistoricalWeather>();

        for (int i = 1; i < 8; i++)
        {
            var date = DateTime.UtcNow.AddDays(-i);
            var historicalWeather = new HistoricalWeather
            {
                Location = new Location { Name = city },
                Forecast = new Forecast { ForecastDays = new List<ForecastDay> { new ForecastDay { Date = date.ToString("yyyy-MM-dd") } }.ToArray() }
            };
            historicalWeatherList.Add(historicalWeather);

            var jsonResponse = JsonConvert.SerializeObject(historicalWeather);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(date.ToString("yyyy-MM-dd"))),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }

        // Act
        var result = await _weatherApiService.GetHistoricalWeatherForPastWeekAsync(city);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.Count);
        for (int i = 0; i < 7; i++)
        {
            Assert.Equal(city, result[i].Location.Name);
            Assert.Equal(DateTime.UtcNow.AddDays(-(i + 1)).ToString("yyyy-MM-dd"), result[i].Forecast.ForecastDays[0].Date);
        }
    }


    //[Fact]
    //public async Task GetHistoricalWeatherForPastWeekAsync_InvalidLocation_ReturnsPartialList()
    //{
    //    // Arrange
    //    var city = "Paris";
    //    var successfulDays = 5;
    //    var failedDays = 2;
    //    var historicalWeatherList = new List<HistoricalWeather>();

    //    for (int i = 1; i <= successfulDays; i++)
    //    {
    //        var date = DateTime.UtcNow.AddDays(-i);
    //        var historicalWeather = new HistoricalWeather
    //        {
    //            Location = new Location { Name = city },
    //            Forecast = new Forecast { ForecastDays = new List<ForecastDay> { new ForecastDay { Date = date.ToString("yyyy-MM-dd") } }.ToArray() }
    //        };
    //        historicalWeatherList.Add(historicalWeather);

    //        var jsonResponse = JsonConvert.SerializeObject(historicalWeather);
    //        var response = new HttpResponseMessage(HttpStatusCode.OK)
    //        {
    //            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
    //        };

    //        _mockHttpMessageHandler.Protected()
    //            .Setup<Task<HttpResponseMessage>>(
    //                "SendAsync",
    //                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(date.ToString("yyyy-MM-dd"))),
    //                ItExpr.IsAny<CancellationToken>()
    //            )
    //            .ReturnsAsync(response);
    //    }

    //    var failedResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

    //    for (int i = successfulDays + 1; i <= successfulDays + failedDays; i++)
    //    {
    //        var date = DateTime.UtcNow.AddDays(-i);
    //        _mockHttpMessageHandler.Protected()
    //            .Setup<Task<HttpResponseMessage>>(
    //                "SendAsync",
    //                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(date.ToString("yyyy-MM-dd"))),
    //                ItExpr.IsAny<CancellationToken>()
    //            )
    //            .ReturnsAsync(failedResponse);
    //    }

    //    // Act
    //    var result = await _weatherApiService.GetHistoricalWeatherForPastWeekAsync(city);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(successfulDays, result.Count);
    //    for (int i = 0; i < successfulDays; i++)
    //    {
    //        Assert.Equal(city, result[i].Location.Name);
    //        Assert.Equal(DateTime.UtcNow.AddDays(-(i + 1)).ToString("yyyy-MM-dd"), result[i].Forecast.ForecastDays[0].Date);
    //    }
    //}



    //[Fact]
    //public async Task GetHistoricalWeatherForPastWeekAsync_Non200StatusCode_ReturnsPartialList()
    //{
    //    // Arrange
    //    var city = "Paris";
    //    var successfulDays = 5;
    //    var failedDays = 2;
    //    var historicalWeatherList = new List<HistoricalWeather>();

    //    for (int i = 1; i <= successfulDays; i++)
    //    {
    //        var date = DateTime.UtcNow.AddDays(-i);
    //        var historicalWeather = new HistoricalWeather
    //        {
    //            Location = new Location { Name = city },
    //            Forecast = new Forecast { ForecastDays = new List<ForecastDay> { new ForecastDay { Date = date.ToString("yyyy-MM-dd") } }.ToArray() }
    //        };
    //        historicalWeatherList.Add(historicalWeather);

    //        var jsonResponse = JsonConvert.SerializeObject(historicalWeather);
    //        var response = new HttpResponseMessage(HttpStatusCode.OK)
    //        {
    //            Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
    //        };

    //        _mockHttpMessageHandler.Protected()
    //            .Setup<Task<HttpResponseMessage>>(
    //                "SendAsync",
    //                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(date.ToString("yyyy-MM-dd"))),
    //                ItExpr.IsAny<CancellationToken>()
    //            )
    //            .ReturnsAsync(response);
    //    }

    //    for (int i = successfulDays + 1; i <= successfulDays + failedDays; i++)
    //    {
    //        var date = DateTime.UtcNow.AddDays(-i);
    //        var failedResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

    //        _mockHttpMessageHandler.Protected()
    //            .Setup<Task<HttpResponseMessage>>(
    //                "SendAsync",
    //                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(date.ToString("yyyy-MM-dd"))),
    //                ItExpr.IsAny<CancellationToken>()
    //            )
    //            .ReturnsAsync(failedResponse);
    //    }

    //    // Act
    //    var result = await _weatherApiService.GetHistoricalWeatherForPastWeekAsync(city);

    //    // Assert
    //    Assert.NotNull(result);
    //    Assert.Equal(successfulDays, result.Count);
    //    for (int i = 0; i < successfulDays; i++)
    //    {
    //        Assert.Equal(city, result[i].Location.Name);
    //        Assert.Equal(DateTime.UtcNow.AddDays(-(i + 1)).ToString("yyyy-MM-dd"), result[i].Forecast.ForecastDays[0].Date);
    //    }
    //}


    [Fact]
    public async Task GetForecastAsync_ValidLocation_ReturnsForecastWeather()
    {
        // Arrange
        var city = "London";
        var expectedResponse = new ForecastWeather
        {
            Forecast = new Forecast
            {
                ForecastDays = new List<ForecastDay>
                {
                    new ForecastDay { Date = DateTime.UtcNow.ToString("yyyy-MM-dd"), Day = new Day { AvgTempC = 15.0 } }
                }.ToArray()
            }
        };
        var jsonResponse = JsonConvert.SerializeObject(expectedResponse);
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
        var result = await _weatherApiService.GetForecastAsync(city);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Forecast);
        Assert.Single(result.Forecast.ForecastDays);
        Assert.Equal(DateTime.UtcNow.ToString("yyyy-MM-dd"), result.Forecast.ForecastDays[0].Date);
        Assert.Equal(15.0, result.Forecast.ForecastDays[0].Day.AvgTempC);
    }

    [Fact]
    public async Task GetForecastAsync_InvalidLocation_ReturnsNull()
    {
        // Arrange
        var city = "InvalidLocation";
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _weatherApiService.GetForecastAsync(city);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetForecastAsync_Non200StatusCode_ReturnsNull()
    {
        // Arrange
        var city = "London";
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _weatherApiService.GetForecastAsync(city);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWeatherAsync_EmptyResponse_ReturnsNull()
    {
        // Arrange
        var location = "Seattle";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
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

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetHistoricalWeatherAsync_EmptyResponse_ReturnsNull()
    {
        // Arrange
        var city = "London";
        var date = DateTime.UtcNow.AddDays(-1);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _weatherApiService.GetHistoricalWeatherAsync(city, date);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetForecastAsync_EmptyResponse_ReturnsNull()
    {
        // Arrange
        var city = "London";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await _weatherApiService.GetForecastAsync(city);

        // Assert
        Assert.Null(result);
    }
}
