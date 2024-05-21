using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WeatherApp.Model;

public class PremiumModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PremiumModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public bool IsPremium { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            IsPremium = user?.IsPremium ?? false;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.IsPremium = true;
                await _userManager.UpdateAsync(user);
            }
        }
        return RedirectToPage("/Index");
    }
}
