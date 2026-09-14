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
    public class QuizHubController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserVocabularyService _vocab;
        private readonly IStudyActivityService _study;
        private readonly IProductHubService _product;

        public QuizHubController(
            ApplicationDbContext context,
            IUserVocabularyService vocab,
            IStudyActivityService study,
            IProductHubService product)
        {
            _context = context;
            _vocab = vocab;
            _study = study;
            _product = product;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Start(string kind = "mixed")
        {
            var model = await BuildAsync(kind);
            if (model.Items.Count == 0)
            {
                TempData["error"] = "Chưa đủ câu hỏi. Hãy học thêm từ hoặc ngữ pháp.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(string kind, Dictionary<int, string> answers)
        {
            var model = await BuildAsync(kind);
            var userId = GetUserId();
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
                else
                {
                    await _product.RecordMistakeAsync(
                        userId,
                        item.Type,
                        item.Prompt,
                        item.Answer,
                        chosen);
                }
            }

            model.Score = correct;
            model.Total = model.Items.Count;

            if (correct > 0)
            {
                await _study.TrackAsync(userId, points: correct, minutes: 6);
            }

            return View(model);
        }

        public async Task<IActionResult> Pronounce()
        {
            var words = (await _vocab.GetAllAsync(GetUserId())).Where(x => x.Word != null).ToList();
            var item = words.Count == 0
                ? null
                : words[Random.Shared.Next(words.Count)];

            if (item?.Word == null)
            {
                TempData["error"] = "Hãy thêm từ vào sổ trước khi làm pronunciation quiz.";
                return RedirectToAction(nameof(Index));
            }

            return View(new SpeakingPromptViewModel
            {
                Mode = "pronounce",
                Title = item.Word.WordText,
                Expected = item.Word.WordText,
                Hint = item.Word.Definition ?? item.Word.Meanings.FirstOrDefault()?.Meaning ?? "",
                Pronunciation = item.Word.Pronunciation
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pronounce(string expected, string heardText, int? durationMs)
        {
            var assessment = SpeechFeedback.Assess(expected, heardText, null, durationMs);
            var model = new SpeakingPromptViewModel
            {
                Mode = "pronounce",
                Title = expected,
                Expected = expected,
                HeardText = heardText,
                DurationMs = durationMs,
                ScorePercent = assessment.Overall,
                IsPassed = assessment.Overall >= 70,
                Assessment = assessment
            };

            if (model.IsPassed == true)
            {
                await _study.TrackAsync(GetUserId(), points: 2, minutes: 1);
            }
            else
            {
                await _product.RecordMistakeAsync(GetUserId(), "pronounce", expected, expected, heardText);
            }

            return View(model);
        }

        public async Task<IActionResult> Review()
        {
            return View(await _product.GetOpenMistakesAsync(GetUserId()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReviewed(int id)
        {
            await _product.MarkMistakeReviewedAsync(GetUserId(), id);
            TempData["success"] = "Đã đánh dấu đã ôn.";
            return RedirectToAction(nameof(Review));
        }

        private async Task<QuizHubPlayViewModel> BuildAsync(string kind)
        {
            var items = new List<ReadingPassageItem>();

            if (kind is "mixed" or "grammar" or "challenge" or "exam")
            {
                var grammar = await _context.GrammarItems
                    .AsNoTracking()
                    .Include(x => x.Topic)
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(kind == "grammar" ? 8 : 4)
                    .ToListAsync();

                items.AddRange(grammar.Select(x => new ReadingPassageItem
                {
                    QuestionId = x.Id,
                    Type = "grammar",
                    Prompt = x.QuestionText,
                    Options = (x.Options ?? "")
                        .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .ToList(),
                    Answer = x.Answer
                }));
            }

            if (kind is "mixed" or "vocab" or "exam" or "challenge")
            {
                var words = (await _vocab.GetAllAsync(GetUserId()))
                    .Where(x => x.Word != null)
                    .Take(20)
                    .ToList();

                foreach (var word in words.OrderBy(_ => Random.Shared.Next()).Take(kind == "vocab" ? 8 : 4))
                {
                    var meaning = word.Word!.Meanings.FirstOrDefault()?.Meaning
                        ?? word.Word.Definition
                        ?? word.Word.WordText;
                    items.Add(new ReadingPassageItem
                    {
                        QuestionId = 10_000 + word.WordId,
                        Type = "vocab",
                        Prompt = $"Nghĩa của “{word.Word.WordText}”?",
                        Options = BuildChoices(meaning, words.Select(x =>
                            x.Word?.Meanings.FirstOrDefault()?.Meaning ?? x.Word?.Definition ?? x.Word?.WordText ?? "")),
                        Answer = meaning
                    });
                }
            }

            if (kind is "scramble")
            {
                var examples = await _context.wordExamples
                    .AsNoTracking()
                    .Where(x => x.EnglishSentence.Length > 8)
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(6)
                    .ToListAsync();

                foreach (var example in examples)
                {
                    var words = example.EnglishSentence
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    items.Add(new ReadingPassageItem
                    {
                        QuestionId = 20_000 + example.Id,
                        Type = "scramble",
                        Prompt = "Sắp xếp thành câu: " + string.Join(" / ", words.OrderBy(_ => Random.Shared.Next())),
                        Options = new List<string> { example.EnglishSentence },
                        Answer = example.EnglishSentence
                    });
                }
            }

            return new QuizHubPlayViewModel
            {
                Kind = kind,
                Timed = kind is "challenge" or "exam",
                Seconds = kind == "exam" ? 600 : 180,
                Items = items.Take(8).ToList()
            };
        }

        private static List<string> BuildChoices(string answer, IEnumerable<string> pool)
        {
            var others = pool
                .Where(x => !string.IsNullOrWhiteSpace(x)
                    && !string.Equals(x, answer, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .Take(3)
                .ToList();

            while (others.Count < 3)
            {
                others.Add($"Lựa chọn {others.Count + 1}");
            }

            return others.Append(answer).OrderBy(_ => Random.Shared.Next()).ToList();
        }

        private string GetUserId() =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
    }

    public class QuizHubPlayViewModel
    {
        public string Kind { get; set; } = "mixed";

        public bool Timed { get; set; }

        public int Seconds { get; set; }

        public List<ReadingPassageItem> Items { get; set; } = new();

        public int? Score { get; set; }

        public int? Total { get; set; }
    }
}
