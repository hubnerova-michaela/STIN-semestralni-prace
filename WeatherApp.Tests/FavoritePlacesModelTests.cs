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
using WeatherApp.Pages;
using WeatherApp.Services;
using Xunit;

public class FavoritePlacesModelTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IWeatherApiService> _mockWeatherApiService;

    public FavoritePlacesModelTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
        _mockWeatherApiService = new Mock<IWeatherApiService>();
    }

    private ApplicationDbContext GetInMemoryDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private void SetUserClaims(FavoritePlacesModel pageModel, string userId, bool isAuthenticated)
    {
        ClaimsPrincipal claimsPrincipal;

        if (isAuthenticated && userId != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims, "mock");
            claimsPrincipal = new ClaimsPrincipal(identity);
        }
        else
        {
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        }

        pageModel.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task OnGetAsync_UserNotPremiumOrNotAuthenticated_RedirectsToIndex()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnGetAsync_UserNotPremiumOrNotAuthenticated");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await pageModel.OnGetAsync();

        // Assert
        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirectToPageResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_PopulatesFavoritePlacesAndForecasts()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnGetAsync_PopulatesFavoritePlacesAndForecasts");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        var user = new ApplicationUser { Id = "user1", IsPremium = true };
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var favoritePlaces = new List<FavoritePlace>
        {
            new FavoritePlace { Id = 1, City = "Paris", UserId = "user1" },
            new FavoritePlace { Id = 2, City = "Berlin", UserId = "user1" }
        };

        await dbContext.FavoritePlaces.AddRangeAsync(favoritePlaces);
        await dbContext.SaveChangesAsync();

        var forecast1 = new ForecastWeather { Forecast = new Forecast { ForecastDays = new ForecastDay[] { } } };
        var forecast2 = new ForecastWeather { Forecast = new Forecast { ForecastDays = new ForecastDay[] { } } };
        _mockWeatherApiService.Setup(ws => ws.GetForecastAsync("Paris")).ReturnsAsync(forecast1);
        _mockWeatherApiService.Setup(ws => ws.GetForecastAsync("Berlin")).ReturnsAsync(forecast2);

        // Act
        SetUserClaims(pageModel, "user1", true);
        await pageModel.OnGetAsync();

        // Assert
        Assert.NotNull(pageModel.FavoritePlaces);
        Assert.Equal(2, pageModel.FavoritePlaces.Count);
        Assert.Equal("Paris", pageModel.FavoritePlaces[0].City);
        Assert.Equal("Berlin", pageModel.FavoritePlaces[1].City);
        Assert.Equal(forecast1.Forecast, pageModel.FavoritePlacesForecasts["Paris"]);
        Assert.Equal(forecast2.Forecast, pageModel.FavoritePlacesForecasts["Berlin"]);
    }

    [Fact]
    public async Task OnPostDeleteAsync_RemovesFavoritePlace()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostDeleteAsync_RemovesFavoritePlace");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        var favoritePlace = new FavoritePlace { Id = 1, City = "Paris", UserId = "user1" };
        await dbContext.FavoritePlaces.AddAsync(favoritePlace);
        await dbContext.SaveChangesAsync();

        // Act
        SetUserClaims(pageModel, "user1", true);
        var result = await pageModel.OnPostDeleteAsync(1);

        // Assert
        var removedPlace = await dbContext.FavoritePlaces.FindAsync(1);
        Assert.Null(removedPlace);
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostSaveFavoriteAsync_AddsNewFavoritePlace()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostSaveFavoriteAsync_AddsNewFavoritePlace");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        var user = new ApplicationUser { Id = "user1", IsPremium = true };
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        // Act
        SetUserClaims(pageModel, "user1", true);
        var result = await pageModel.OnPostSaveFavoriteAsync("Paris");

        // Assert
        var favoritePlace = await dbContext.FavoritePlaces.FirstOrDefaultAsync(fp => fp.City == "Paris" && fp.UserId == "user1");
        Assert.NotNull(favoritePlace);
        Assert.Equal("Paris", favoritePlace.City);
        Assert.Equal("user1", favoritePlace.UserId);
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostSaveFavoriteAsync_ReturnsUnauthorizedIfUserNotPremium()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostSaveFavoriteAsync_ReturnsUnauthorizedIfUserNotPremium");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        var user = new ApplicationUser { Id = "user1", IsPremium = false };
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        // Act
        SetUserClaims(pageModel, "user1", true);
        var result = await pageModel.OnPostSaveFavoriteAsync("Paris");

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_NonAuthenticatedUser_RedirectsToIndex()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnGetAsync_NonAuthenticatedUser_RedirectsToIndex");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);
        SetUserClaims(pageModel, null, false);

        // Act
        var result = await pageModel.OnGetAsync();

        // Assert
        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirectToPageResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_AuthenticatedNonPremiumUser_RedirectsToIndex()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnGetAsync_AuthenticatedNonPremiumUser_RedirectsToIndex");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        var user = new ApplicationUser { Id = "user1", IsPremium = false };
        SetUserClaims(pageModel, user.Id, true);
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        // Act
        var result = await pageModel.OnGetAsync();

        // Assert
        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", redirectToPageResult.PageName);
    }

    [Fact]
    public async Task OnGetAsync_NoFavoritePlaces_SetsEmptyFavoritePlacesList()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnGetAsync_NoFavoritePlaces_SetsEmptyFavoritePlacesList");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        var user = new ApplicationUser { Id = "user1", IsPremium = true };
        SetUserClaims(pageModel, user.Id, true);
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        // Act
        await pageModel.OnGetAsync();

        // Assert
        Assert.Empty(pageModel.FavoritePlaces);
    }

    [Fact]
    public async Task OnPostDeleteAsync_NonExistingFavoritePlace_ReturnsNotFound()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostDeleteAsync_NonExistingFavoritePlace_ReturnsNotFound");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        // Act
        var result = await pageModel.OnPostDeleteAsync(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task OnPostSaveFavoriteAsync_CityAlreadyFavorite_DoesNotAddDuplicate()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostSaveFavoriteAsync_CityAlreadyFavorite_DoesNotAddDuplicate");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        var user = new ApplicationUser { Id = "user1", IsPremium = true };
        SetUserClaims(pageModel, user.Id, true);
        await dbContext.Users.AddAsync(user);
        await dbContext.FavoritePlaces.AddAsync(new FavoritePlace { City = "Paris", UserId = user.Id });
        await dbContext.SaveChangesAsync();

        // Act
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        var result = await pageModel.OnPostSaveFavoriteAsync("Paris");

        // Assert
        var favoritePlaces = await dbContext.FavoritePlaces.Where(fp => fp.UserId == user.Id).ToListAsync();
        Assert.Single(favoritePlaces);
        Assert.Equal("Paris", favoritePlaces[0].City);
        Assert.IsType<RedirectToPageResult>(result);
    }

    [Fact]
    public async Task OnPostSaveFavoriteAsync_NonAuthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        var dbContext = GetInMemoryDbContext("TestDatabase_OnPostSaveFavoriteAsync_NonAuthenticatedUser_ReturnsUnauthorized");
        var pageModel = new FavoritePlacesModel(dbContext, _mockUserManager.Object, _mockWeatherApiService.Object);

        // Act
        var result = await pageModel.OnPostSaveFavoriteAsync("Paris");

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }
}
