using AppLearningEnglish.Business;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CoachController : Controller
    {
        private readonly IUserVocabularyService _vocab;
        private readonly ILearnerWorkspaceService _workspace;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _users;

        public CoachController(
            IUserVocabularyService vocab,
            ILearnerWorkspaceService workspace,
            ApplicationDbContext context,
            UserManager<ApplicationUser> users)
        {
            _vocab = vocab;
            _workspace = workspace;
            _context = context;
            _users = users;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _users.GetUserAsync(User);
            var words = (await _vocab.GetAllAsync(GetUserId())).ToList();
            var skills = await _workspace.GetSkillsAsync(GetUserId());
            ViewBag.WordCount = words.Count;
            ViewBag.Level = user?.EnglishLevel;
            ViewBag.Goal = user?.LearningGoalKind;
            ViewBag.Skills = skills;
            ViewBag.Weak = skills.WeakList.Take(5).Select(x => x.Word?.WordText).Where(x => x != null);
            return View(words.Take(8).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Example(int wordId)
        {
            var item = await _vocab.GetAsync(GetUserId(), wordId);
            var user = await _users.GetUserAsync(User);
            TempData["coach"] = item?.Word == null
                ? "Không thấy từ."
                : CoachEngine.Example(item.Word, user?.EnglishLevel);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult QuizFromVocab() =>
            RedirectToAction("Start", "QuizHub", new { area = "Customer", kind = "vocab" });

        public async Task<IActionResult> Suggest()
        {
            var user = await _users.GetUserAsync(User);
            var goal = user?.LearningGoalKind ?? "vocab";
            var slug = goal switch
            {
                "ielts" => "school",
                "workplace" => "work",
                "communication" => "friends",
                "exam" => "school",
                _ => "daily-routines"
            };
            var words = await _context.words.AsNoTracking()
                .Where(x => x.TopicSlug == slug)
                .Take(12)
                .ToListAsync();
            ViewBag.Goal = goal;
            return View(words);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask(string question)
        {
            var topics = await _context.GrammarTopics.AsNoTracking().ToListAsync();
            TempData["coach"] = CoachEngine.Answer(question, topics);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Weakness()
        {
            var skills = await _workspace.GetSkillsAsync(GetUserId());
            return View(skills);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Translate(string text)
        {
            TempData["coach"] = CoachEngine.Translate(text);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Synonyms(string left, string right)
        {
            TempData["coach"] = CoachEngine.SynonymDiff(left ?? "say", right ?? "tell");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Story()
        {
            var words = (await _vocab.GetAllAsync(GetUserId()))
                .Select(x => x.Word?.WordText ?? "")
                .Where(x => x.Length > 0)
                .Take(8);
            TempData["coach"] = CoachEngine.Story(words);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Weekly()
        {
            var skills = await _workspace.GetSkillsAsync(GetUserId());
            TempData["coach"] =
                $"Tuần này: streak {skills.CurrentStreak} ngày, {skills.MasteredWords}/{skills.TotalWords} từ thuộc, " +
                $"quiz TB {skills.ExerciseAverage}%, nghe {skills.ListeningPercent}%, viết {skills.WritingPercent}%. " +
                "Hãy ôn từ yếu và làm Final Test level hiện tại.";
            return RedirectToAction(nameof(Index));
        }

        private string GetUserId() =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
    }
}
