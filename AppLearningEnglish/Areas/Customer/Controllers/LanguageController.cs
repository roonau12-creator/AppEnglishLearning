using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppLearningEnglish.Models;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class LanguageController : Controller
    {
        private readonly UserManager<ApplicationUser> _users;

        public LanguageController(UserManager<ApplicationUser> users)
        {
            _users = users;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Set(string lang, string? returnUrl = null)
        {
            lang = lang == "en" ? "en" : "vi";
            Response.Cookies.Append(AppUi.CookieName, lang, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/"
            });

            var user = await _users.GetUserAsync(User);
            if (user != null)
            {
                user.UiLanguage = lang;
                await _users.UpdateAsync(user);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }
}
