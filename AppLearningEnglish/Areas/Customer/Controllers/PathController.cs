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
    public class PathController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStudyActivityService _study;
        private readonly IProductHubService _product;

        public PathController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IStudyActivityService study,
            IProductHubService product)
        {
            _context = context;
            _userManager = userManager;
            _study = study;
            _product = product;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var level = user?.EnglishLevel;
            var grammar = await _product.GetGrammarProgressAsync(_userManager.GetUserId(User)!);
            var topics = await _context.GrammarTopics.AsNoTracking().ToListAsync();

            var nodes = EnglishLevelScale.CefrAll.Select(cefr =>
            {
                var group = topics.Where(x => EnglishLevelScale.Normalize(x.Level) == cefr).ToList();
                var percents = group
                    .Select(x => grammar.TryGetValue(x.Id, out var row) ? row.BestPercent : 0)
                    .ToList();
                return new PathLevelViewModel
                {
                    Level = cefr,
                    IsCurrent = EnglishLevelScale.Normalize(level) == cefr,
                    IsLocked = EnglishLevelScale.IsTooHard(level, cefr),
                    TopicCount = group.Count,
                    Progress = percents.Count == 0 ? 0 : (int)Math.Round(percents.Average())
                };
            }).ToList();

            return View(new PathHubViewModel
            {
                UserLevel = EnglishLevelScale.Display(level),
                Levels = nodes
            });
        }

        public async Task<IActionResult> Level(string id)
        {
            var cefr = EnglishLevelScale.Normalize(id);
            if (!EnglishLevelScale.CefrAll.Contains(cefr))
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (EnglishLevelScale.IsTooHard(user?.EnglishLevel, cefr))
            {
                TempData["error"] = "Level này đang khóa. Hãy hoàn thành Final Test level trước.";
                return RedirectToAction(nameof(Index));
            }

            var topics = await _context.GrammarTopics
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.Level == cefr
                    || x.Level == MapLegacy(cefr))
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            if (topics.Count == 0)
            {
                topics = (await _context.GrammarTopics.AsNoTracking().Include(x => x.Items).ToListAsync())
                    .Where(x => EnglishLevelScale.Normalize(x.Level) == cefr)
                    .OrderBy(x => x.SortOrder)
                    .ToList();
            }

            return View(new PathLevelDetailViewModel
            {
                Level = cefr,
                Topics = topics
            });
        }

        public async Task<IActionResult> FinalTest(string id)
        {
            var model = await BuildTestAsync(id);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalTest(string id, Dictionary<int, string> answers)
        {
            var model = await BuildTestAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            var correct = 0;
            foreach (var item in model.Items)
            {
                answers.TryGetValue(item.QuestionId, out var chosen);
                item.UserAnswer = chosen;
                item.IsCorrect = ExerciseGrading.TextAnswerMatches(item.Answer, chosen);
                if (item.IsCorrect == true)
                {
                    correct++;
                }
            }

            model.Score = correct;
            model.Total = model.Items.Count;
            var total = model.Items.Count;
            var percent = total == 0 ? 0 : (int)Math.Round(100d * correct / total);
            model.Passed = percent >= 70;

            var userId = _userManager.GetUserId(User)!;
            if (correct > 0)
            {
                await _study.TrackAsync(userId, points: correct * 2, minutes: 10);
            }

            _context.PracticeAttempts.Add(new PracticeAttempt
            {
                ApplicationUserId = userId,
                Kind = "path",
                Title = $"Final Test {model.Level}",
                Score = percent,
                UserAnswer = $"{correct}/{model.Total}",
                CreatedAt = DateTime.UtcNow
            });

            if (model.Passed)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var next = EnglishLevelScale.Next(model.Level);
                    if (!string.IsNullOrWhiteSpace(next)
                        && EnglishLevelScale.Rank(next) > EnglishLevelScale.Rank(user.EnglishLevel))
                    {
                        user.EnglishLevel = next;
                        await _userManager.UpdateAsync(user);
                        model.UnlockedLevel = next;
                    }
                }

                await _product.NotifyAsync(
                    userId,
                    "achievement",
                    $"Đạt Final Test {model.Level}",
                    model.UnlockedLevel == null
                        ? "Bạn đã vượt bài kiểm tra level."
                        : $"Mở khóa {model.UnlockedLevel}.",
                    "/lo-trinh");
            }

            await _context.SaveChangesAsync();
            return View(model);
        }

        private async Task<PathTestViewModel?> BuildTestAsync(string id)
        {
            var cefr = EnglishLevelScale.Normalize(id);
            if (!EnglishLevelScale.CefrAll.Contains(cefr))
            {
                return null;
            }

            var items = (await _context.GrammarItems
                    .AsNoTracking()
                    .Include(x => x.Topic)
                    .ToListAsync())
                .Where(x => x.Topic != null && EnglishLevelScale.Normalize(x.Topic.Level) == cefr)
                .OrderBy(_ => Random.Shared.Next())
                .Take(8)
                .Select(x => new GrammarDrillItem
                {
                    QuestionId = x.Id,
                    QuestionText = x.QuestionText,
                    Options = (x.Options ?? "")
                        .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .ToList(),
                    Answer = x.Answer,
                    Explanation = x.Explanation
                })
                .ToList();

            if (items.Count == 0)
            {
                return null;
            }

            return new PathTestViewModel { Level = cefr, Items = items };
        }

        private static string MapLegacy(string cefr) => cefr switch
        {
            EnglishLevelScale.A1 => EnglishLevelScale.Beginner,
            EnglishLevelScale.A2 => EnglishLevelScale.Elementary,
            EnglishLevelScale.B1 => EnglishLevelScale.Intermediate,
            EnglishLevelScale.C1 => EnglishLevelScale.Advanced,
            _ => cefr
        };
    }

    public class PathHubViewModel
    {
        public string UserLevel { get; set; } = string.Empty;

        public List<PathLevelViewModel> Levels { get; set; } = new();
    }

    public class PathLevelViewModel
    {
        public string Level { get; set; } = string.Empty;

        public bool IsCurrent { get; set; }

        public bool IsLocked { get; set; }

        public int TopicCount { get; set; }

        public int Progress { get; set; }
    }

    public class PathLevelDetailViewModel
    {
        public string Level { get; set; } = string.Empty;

        public List<GrammarTopic> Topics { get; set; } = new();
    }

    public class PathTestViewModel
    {
        public string Level { get; set; } = string.Empty;

        public List<GrammarDrillItem> Items { get; set; } = new();

        public int? Score { get; set; }

        public int? Total { get; set; }

        public bool Passed { get; set; }

        public string? UnlockedLevel { get; set; }
    }
}
