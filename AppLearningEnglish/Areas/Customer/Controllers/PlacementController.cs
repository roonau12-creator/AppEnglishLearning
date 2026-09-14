using AppLearningEnglish.Business;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class PlacementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseService _courseService;
        private readonly ApplicationDbContext _context;

        public PlacementController(
            UserManager<ApplicationUser> userManager,
            ICourseService courseService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _courseService = courseService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await LoadQuestionsAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Dictionary<int, string> answers)
        {
            var questions = await LoadQuestionsAsync();
            var score = 0;

            foreach (var question in questions)
            {
                answers.TryGetValue(question.Id, out var chosen);

                if (ExerciseGrading.TextAnswerMatches(question.Answer, chosen))
                {
                    score++;
                }
            }

            var level = PlacementBank.LevelForScore(score, questions.Count);
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                user.EnglishLevel = level;
                await _userManager.UpdateAsync(user);
            }

            TempData["success"] = $"Kết quả xếp lớp: {level} ({score}/{questions.Count}).";

            var courses = (await _courseService.GetAllCourseAsync())
                .Where(x => x.IsPublished)
                .ToList();

            return View("Result", new PlacementResultViewModel
            {
                Score = score,
                Total = questions.Count,
                Level = level,
                RecommendedCourses = courses
                    .Where(x => EnglishLevelScale.IsRecommended(level, x.Level))
                    .ToList(),
                LockedCourses = courses
                    .Where(x => EnglishLevelScale.IsTooHard(level, x.Level))
                    .ToList()
            });
        }

        private async Task<List<PlacementQuestion>> LoadQuestionsAsync()
        {
            var items = await _context.PlacementItems
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();

            if (items.Count == 0)
            {
                return PlacementBank.Questions.ToList();
            }

            return items.Select(x => new PlacementQuestion
            {
                Id = x.Id,
                Text = x.Text,
                Options = x.Options.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                Answer = x.Answer
            }).ToList();
        }
    }
}
