using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ILearnerWorkspaceService _workspace;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ICourseService courseService,
            ILearnerWorkspaceService workspace,
            UserManager<ApplicationUser> userManager)
        {
            _courseService = courseService;
            _workspace = workspace;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCourseAsync();
            var featured = courses
                .Where(course => course.IsPublished)
                .OrderByDescending(course => course.CreatedAt)
                .Take(6)
                .ToList();

            var model = new HomePageViewModel
            {
                FeaturedCourses = featured
            };

            var userId = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(userId))
            {
                model.IsSignedIn = true;
                model.Today = await _workspace.GetTodayAsync(userId);
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
