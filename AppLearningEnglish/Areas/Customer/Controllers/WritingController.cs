using AppLearningEnglish.Business;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class WritingController : Controller
    {
        private const int PassScore = 70;

        private readonly IWordExampleService _examples;
        private readonly IStudyActivityService _studyActivity;
        private readonly IUserLessonService _userLessonService;
        private readonly ApplicationDbContext _context;

        public WritingController(
            IWordExampleService examples,
            IStudyActivityService studyActivity,
            IUserLessonService userLessonService,
            ApplicationDbContext context)
        {
            _examples = examples;
            _studyActivity = studyActivity;
            _userLessonService = userLessonService;
            _context = context;
        }

        public async Task<IActionResult> Index(string? genre)
        {
            var items = (await _examples.GetAllWordExampleAsync())
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.EnglishSentence)
                    && !string.IsNullOrWhiteSpace(x.VietnameseMeaning))
                .Take(40)
                .ToList();

            var prompts = await _context.WritingPrompts
                .AsNoTracking()
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Id)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(genre))
            {
                prompts = prompts
                    .Where(x => string.Equals(x.Genre, genre, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(new WritingHubViewModel
            {
                RewriteItems = items,
                Prompts = prompts,
                Genre = genre
            });
        }

        public async Task<IActionResult> History()
        {
            var items = await _context.PracticeAttempts
                .AsNoTracking()
                .Where(x => x.ApplicationUserId == GetUserId() && x.Kind == "writing")
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Practice(int id)
        {
            var model = await BuildRewriteAsync(id);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Practice(int id, string userAnswer)
        {
            var model = await BuildRewriteAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            var review = WritingCoach.ReviewRewrite(model.Expected, userAnswer);
            ApplyReview(model, userAnswer, review);
            await SaveAttemptAsync(model, userAnswer, review);

            if (model.IsPassed == true)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: 3, minutes: 3);
                if (model.Example?.Word?.LessonId is int lessonId)
                {
                    await _userLessonService.MarkWritingDoneAsync(GetUserId(), lessonId);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Prompt(int id)
        {
            var model = await BuildFreeAsync(id);
            return model == null ? NotFound() : View("Practice", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Prompt(int id, string userAnswer)
        {
            var model = await BuildFreeAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            var review = WritingCoach.ReviewFree(
                userAnswer,
                model.FreePrompt!.KeyPoints?.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                model.FreePrompt.SampleAnswer,
                model.FreePrompt.MinWords);

            ApplyReview(model, userAnswer, review);
            await SaveAttemptAsync(model, userAnswer, review);

            if (model.IsPassed == true)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: 5, minutes: 6);
                await _userLessonService.MarkWritingForPromptAsync(GetUserId(), id);
            }

            return View("Practice", model);
        }

        private async Task<WritingPracticeViewModel?> BuildRewriteAsync(int id)
        {
            var example = await _examples.GetWordExampleByIdAsync(id);

            if (example == null)
            {
                return null;
            }

            return new WritingPracticeViewModel
            {
                Example = example,
                Prompt = example.VietnameseMeaning,
                Expected = example.EnglishSentence
            };
        }

        private async Task<WritingPracticeViewModel?> BuildFreeAsync(int id)
        {
            var prompt = await _context.WritingPrompts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (prompt == null)
            {
                return null;
            }

            return new WritingPracticeViewModel
            {
                FreePrompt = prompt,
                Prompt = prompt.Prompt,
                Expected = prompt.SampleAnswer ?? string.Empty
            };
        }

        private async Task SaveAttemptAsync(
            WritingPracticeViewModel model,
            string? userAnswer,
            WritingReview review)
        {
            _context.PracticeAttempts.Add(new PracticeAttempt
            {
                ApplicationUserId = GetUserId(),
                Kind = "writing",
                Title = model.IsFreeWrite ? model.FreePrompt!.Title : (model.Example?.Word?.WordText ?? "Viết câu"),
                Prompt = model.Prompt,
                Expected = model.Expected,
                UserAnswer = userAnswer?.Trim(),
                Score = review.Overall,
                Feedback = string.Join(" · ", review.Comments),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private static void ApplyReview(
            WritingPracticeViewModel model,
            string? userAnswer,
            WritingReview review)
        {
            model.UserAnswer = userAnswer?.Trim();
            model.Review = review;
            model.ScorePercent = review.Overall;
            model.IsPassed = review.Overall >= PassScore;
        }

        private string GetUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
            {
                throw new UnauthorizedAccessException();
            }

            return claim.Value;
        }
    }
}
