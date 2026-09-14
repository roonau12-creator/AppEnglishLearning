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
    public class SpeakingController : Controller
    {
        private const int PassScore = 70;

        private readonly IWordService _wordService;
        private readonly IUserVocabularyService _userVocabularyService;
        private readonly IWordExampleService _examples;
        private readonly ILessonService _lessonService;
        private readonly IStudyActivityService _studyActivity;
        private readonly ILearningAccessService _learningAccess;
        private readonly IUserLessonService _userLessonService;
        private readonly IPronunciationScoringService _pronunciation;
        private readonly ApplicationDbContext _context;

        public SpeakingController(
            IWordService wordService,
            IUserVocabularyService userVocabularyService,
            IWordExampleService examples,
            ILessonService lessonService,
            IStudyActivityService studyActivity,
            ILearningAccessService learningAccess,
            IUserLessonService userLessonService,
            IPronunciationScoringService pronunciation,
            ApplicationDbContext context)
        {
            _wordService = wordService;
            _userVocabularyService = userVocabularyService;
            _examples = examples;
            _lessonService = lessonService;
            _studyActivity = studyActivity;
            _learningAccess = learningAccess;
            _userLessonService = userLessonService;
            _pronunciation = pronunciation;
            _context = context;
        }

        public async Task<IActionResult> History()
        {
            var items = await _context.PracticeAttempts
                .AsNoTracking()
                .Where(x => x.ApplicationUserId == GetUserId() && x.Kind == "speaking")
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Index(int? lessonId)
        {
            var userId = GetUserId();
            IEnumerable<AppLearningEnglish.Models.Word> words;

            if (lessonId.HasValue && lessonId.Value > 0)
            {
                if (!await _learningAccess.IsEnrolledForLessonAsync(userId, lessonId.Value))
                {
                    TempData["error"] = "Hãy đăng ký khóa học trước khi luyện nói bài này.";
                    return RedirectToAction("Index", "Course", new { area = "Customer" });
                }

                words = await _wordService.GetWordsByLessonIdAsync(lessonId.Value);
            }
            else
            {
                var saved = (await _userVocabularyService.GetAllAsync(userId)).ToList();
                if (saved.Count > 0)
                {
                    words = saved
                        .Where(item => item.Word != null)
                        .Select(item => item.Word!)
                        .DistinctBy(word => word.Id)
                        .Take(24);
                }
                else
                {
                    words = (await _wordService.GetAllWordAsync()).Take(24);
                }
            }

            return View(words.ToList());
        }

        public async Task<IActionResult> Practice(int wordId)
        {
            var word = await _wordService.GetWordByIdAsync(wordId);

            if (word == null)
            {
                return NotFound();
            }

            return View(new SpeakingPracticeViewModel { Word = word });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Practice(int wordId, string heardText, int? durationMs)
        {
            var word = await _wordService.GetWordByIdAsync(wordId);

            if (word == null)
            {
                return NotFound();
            }

            var assessment = _pronunciation.Score(
                word.WordText,
                heardText,
                word.Pronunciation,
                durationMs);
            var passed = assessment.Overall >= PassScore;
            var points = passed ? 2 : 0;

            if (passed)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: points, minutes: 1);
            }

            await SaveAttemptAsync(
                "speaking",
                word.WordText,
                word.Pronunciation,
                word.WordText,
                heardText,
                assessment.Overall,
                string.Join(" · ", assessment.Tips));

            if (passed && word.LessonId.HasValue)
            {
                await _userLessonService.MarkSpeakingDoneAsync(GetUserId(), word.LessonId.Value);
            }

            return View(new SpeakingPracticeViewModel
            {
                Word = word,
                HeardText = heardText?.Trim(),
                DurationMs = durationMs,
                ScorePercent = assessment.Overall,
                IsPassed = passed,
                PointsEarned = points,
                Assessment = assessment
            });
        }

        public async Task<IActionResult> Prompt(string mode = "sentence", int? id = null)
        {
            var model = await BuildPromptAsync(mode, id);

            if (model == null)
            {
                TempData["error"] = "Chưa có câu mẫu để luyện nói.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Prompt(string mode, int? id, string heardText, int? durationMs)
        {
            var model = await BuildPromptAsync(mode, id);

            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var assessment = _pronunciation.Score(
                model.Expected,
                heardText,
                model.Pronunciation,
                durationMs);
            var passed = assessment.Overall >= PassScore;

            model.HeardText = heardText?.Trim();
            model.DurationMs = durationMs;
            model.ScorePercent = assessment.Overall;
            model.IsPassed = passed;
            model.Assessment = assessment;

            if (passed)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: 3, minutes: 2);
            }

            await SaveAttemptAsync(
                "speaking",
                model.Title,
                model.Hint,
                model.Expected,
                heardText,
                assessment.Overall,
                string.Join(" · ", assessment.Tips));

            return View(model);
        }

        private async Task<SpeakingPromptViewModel?> BuildPromptAsync(string? mode, int? id)
        {
            mode = mode switch
            {
                "dialogue" => "dialogue",
                "passage" => "passage",
                _ => "sentence"
            };

            if (mode == "passage")
            {
                var lessons = (await _lessonService.GetAllLessonAsync())
                    .Where(x => x.IsPublished && !string.IsNullOrWhiteSpace(x.Content))
                    .ToList();

                var lesson = id.HasValue
                    ? lessons.FirstOrDefault(x => x.Id == id.Value)
                    : lessons.FirstOrDefault();

                if (lesson == null)
                {
                    return null;
                }

                var passage = lesson.Content!.Trim();
                if (passage.Length > 280)
                {
                    passage = passage[..280].Trim() + "…";
                }

                return new SpeakingPromptViewModel
                {
                    Mode = mode,
                    Title = lesson.Title,
                    Expected = passage,
                    Hint = "Đọc thành tiếng đoạn dưới đây. Hệ thống chỉ từng từ sai, trọng âm và ngữ điệu."
                };
            }

            var examples = (await _examples.GetAllWordExampleAsync())
                .Where(x => !string.IsNullOrWhiteSpace(x.EnglishSentence))
                .ToList();

            if (examples.Count == 0)
            {
                return null;
            }

            if (mode == "dialogue")
            {
                var first = id.HasValue
                    ? examples.FirstOrDefault(x => x.Id == id.Value) ?? examples[0]
                    : examples[0];
                var second = examples.FirstOrDefault(x => x.Id != first.Id) ?? first;
                var text = $"A: {first.EnglishSentence}\nB: {second.EnglishSentence}";

                return new SpeakingPromptViewModel
                {
                    Mode = mode,
                    Title = "Hội thoại ngắn",
                    Expected = text,
                    Hint = "Nói cả hai lượt A và B. Giữ ngữ điệu hội thoại."
                };
            }

            var example = id.HasValue
                ? examples.FirstOrDefault(x => x.Id == id.Value) ?? examples[0]
                : examples[0];

            return new SpeakingPromptViewModel
            {
                Mode = mode,
                Title = example.Word?.WordText ?? "Câu mẫu",
                Expected = example.EnglishSentence,
                Hint = example.VietnameseMeaning,
                Pronunciation = example.Word?.Pronunciation
            };
        }

        private async Task SaveAttemptAsync(
            string kind,
            string title,
            string? prompt,
            string? expected,
            string? answer,
            int score,
            string? feedback)
        {
            _context.PracticeAttempts.Add(new PracticeAttempt
            {
                ApplicationUserId = GetUserId(),
                Kind = kind,
                Title = title,
                Prompt = prompt,
                Expected = expected,
                UserAnswer = answer?.Trim(),
                Score = score,
                Feedback = feedback,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
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
