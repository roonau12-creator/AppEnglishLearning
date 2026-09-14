using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ThemeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ThemeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(string? returnUrl = null)
        {
            var current = Request.Cookies["app-theme"] == "dark" ? "dark" : "light";
            var next = current == "dark" ? "light" : "dark";

            Response.Cookies.Append("app-theme", next, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/"
            });

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.PreferredTheme = next;
                await _userManager.UpdateAsync(user);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }
}
