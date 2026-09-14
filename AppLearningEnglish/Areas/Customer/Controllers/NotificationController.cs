using AppLearningEnglish.Business.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IProductHubService _product;

        public NotificationController(IProductHubService product)
        {
            _product = product;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            return View(await _product.GetInboxAsync(userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Read(int? id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            await _product.MarkReadAsync(userId, id);
            return RedirectToAction(nameof(Index));
        }
    }
}
