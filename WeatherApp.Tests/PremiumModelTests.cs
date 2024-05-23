using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using WeatherApp.Model;
using Xunit;

public class PremiumModelTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public PremiumModelTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStore.Object, null, null, null, null, null, null, null, null);
    }

    private void SetUserClaims(PremiumModel pageModel, string userId, bool isAuthenticated)
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
    public async Task OnGetAsync_AuthenticatedUser_SetsIsPremium()
    {
        // Arrange
        var pageModel = new PremiumModel(_mockUserManager.Object);
        var user = new ApplicationUser { Id = "user1", IsPremium = true };
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        SetUserClaims(pageModel, user.Id, true);

        // Act
        var result = await pageModel.OnGetAsync();

        // Assert
        Assert.True(pageModel.IsPremium);
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnGetAsync_UnauthenticatedUser_DoesNotSetIsPremium()
    {
        // Arrange
        var pageModel = new PremiumModel(_mockUserManager.Object);
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);

        SetUserClaims(pageModel, null, false);

        // Act
        var result = await pageModel.OnGetAsync();

        // Assert
        Assert.False(pageModel.IsPremium);
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPostAsync_AuthenticatedUser_SetsIsPremiumToTrue()
    {
        // Arrange
        var pageModel = new PremiumModel(_mockUserManager.Object);
        var user = new ApplicationUser { Id = "user1", IsPremium = false };
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        SetUserClaims(pageModel, user.Id, true);

        // Act
        var result = await pageModel.OnPostAsync();

        // Assert
        _mockUserManager.Verify(um => um.UpdateAsync(It.Is<ApplicationUser>(u => u.IsPremium == true)), Times.Once);
        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", ((RedirectToPageResult)result).PageName);
    }

    [Fact]
    public async Task OnPostAsync_UnauthenticatedUser_DoesNotSetIsPremium()
    {
        // Arrange
        var pageModel = new PremiumModel(_mockUserManager.Object);
        _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);

        SetUserClaims(pageModel, null, false);

        // Act
        var result = await pageModel.OnPostAsync();

        // Assert
        _mockUserManager.Verify(um => um.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Index", ((RedirectToPageResult)result).PageName);
    }
}
