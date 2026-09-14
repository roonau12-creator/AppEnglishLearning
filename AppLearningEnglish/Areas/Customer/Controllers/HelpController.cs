using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
