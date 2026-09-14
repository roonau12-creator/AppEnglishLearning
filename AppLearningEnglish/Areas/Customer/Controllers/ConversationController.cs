using AppLearningEnglish.Business;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ConversationController : Controller
    {
        private readonly IStudyActivityService _studyActivity;
        private readonly ApplicationDbContext _context;

        public ConversationController(
            IStudyActivityService studyActivity,
            ApplicationDbContext context)
        {
            _studyActivity = studyActivity;
            _context = context;
        }

        public IActionResult Index()
        {
            return View(ConversationBank.All);
        }

        public IActionResult FreeTalk(int turn = 0, string? last = null)
        {
            var current = FreeTalkEngine.Next(last, turn);
            return View(new ConversationPlayViewModel
            {
                Scenario = new ConversationScenario
                {
                    Id = "freetalk",
                    Title = "AI hội thoại tự do (kịch bản)",
                    Level = "A2",
                    Setting = "Gia sư hỏi theo chủ đề bạn nêu. Đây là nhánh kịch bản, không phải mô hình ngôn ngữ."
                },
                TurnIndex = turn,
                Current = current
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(FreeTalk))]
        public async Task<IActionResult> FreeTalkReply(int turn, string reply)
        {
            var current = FreeTalkEngine.Next(null, turn);
            var score = ConversationBank.ScoreReply(current, reply);
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            _context.PracticeAttempts.Add(new PracticeAttempt
            {
                ApplicationUserId = userId,
                Kind = "conversation",
                Title = "Free talk",
                Prompt = current.Tutor,
                Expected = current.Sample,
                UserAnswer = reply?.Trim(),
                Score = score,
                Feedback = score >= 60 ? "Đủ ý. Tiếp tục kể thêm." : "Thêm từ khóa hoặc because + lý do.",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            if (score >= 60)
            {
                await _studyActivity.TrackAsync(userId, points: 2, minutes: 2);
            }

            return View(new ConversationPlayViewModel
            {
                Scenario = new ConversationScenario
                {
                    Id = "freetalk",
                    Title = "AI hội thoại tự do (kịch bản)",
                    Level = "A2",
                    Setting = "Gia sư hỏi theo chủ đề bạn nêu."
                },
                TurnIndex = turn,
                Current = current,
                Reply = reply?.Trim(),
                Score = score,
                IsPassed = score >= 60,
                IsLast = turn >= 4
            });
        }

        public IActionResult Play(string id, int turn = 0)
        {
            var scene = ConversationBank.Find(id);
            if (scene == null)
            {
                return NotFound();
            }

            turn = Math.Clamp(turn, 0, scene.Turns.Count - 1);
            return View(new ConversationPlayViewModel
            {
                Scenario = scene,
                TurnIndex = turn,
                Current = scene.Turns[turn]
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Play(string id, int turn, string reply)
        {
            var scene = ConversationBank.Find(id);
            if (scene == null)
            {
                return NotFound();
            }

            turn = Math.Clamp(turn, 0, scene.Turns.Count - 1);
            var current = scene.Turns[turn];
            var score = ConversationBank.ScoreReply(current, reply);
            var passed = score >= 60;

            _context.PracticeAttempts.Add(new PracticeAttempt
            {
                ApplicationUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value,
                Kind = "conversation",
                Title = $"{scene.Title} · lượt {turn + 1}",
                Prompt = current.Tutor,
                Expected = current.Sample,
                UserAnswer = reply?.Trim(),
                Score = score,
                Feedback = passed
                    ? "Đủ ý cho lượt này. Nghe câu tiếp."
                    : "Thiếu từ khóa. Xem gợi ý rồi nói lại.",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            if (passed)
            {
                await _studyActivity.TrackAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value,
                    points: 2,
                    minutes: 2);
            }

            return View(new ConversationPlayViewModel
            {
                Scenario = scene,
                TurnIndex = turn,
                Current = current,
                Reply = reply?.Trim(),
                Score = score,
                IsPassed = passed,
                IsLast = turn >= scene.Turns.Count - 1
            });
        }
    }
}
