using AppLearningEnglish.Business.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ChallengeController : Controller
    {
        private readonly IProductHubService _product;

        public ChallengeController(IProductHubService product)
        {
            _product = product;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            ViewBag.Player = await _product.GetPlayerAsync(userId);
            return View(await _product.GetActiveChallengesAsync(userId));
        }
    }
}
