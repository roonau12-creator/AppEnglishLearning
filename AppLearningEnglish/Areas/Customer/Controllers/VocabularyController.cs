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
    public class VocabularyController : Controller
    {
        private readonly IUserVocabularyService _userVocabularyService;
        private readonly IWordService _wordService;
        private readonly ILessonService _lessonService;
        private readonly IUserLessonService _userLessonService;
        private readonly IUserCourseService _userCourseService;
        private readonly IStudyActivityService _studyActivity;
        private readonly ILearningAccessService _learningAccess;
        private readonly ApplicationDbContext _context;

        public VocabularyController(
            IUserVocabularyService userVocabularyService,
            IWordService wordService,
            ILessonService lessonService,
            IUserLessonService userLessonService,
            IUserCourseService userCourseService,
            IStudyActivityService studyActivity,
            ILearningAccessService learningAccess,
            ApplicationDbContext context)
        {
            _userVocabularyService = userVocabularyService;
            _wordService = wordService;
            _lessonService = lessonService;
            _userLessonService = userLessonService;
            _userCourseService = userCourseService;
            _studyActivity = studyActivity;
            _learningAccess = learningAccess;
            _context = context;
        }

        // =====================================
        // MY VOCABULARY
        // =====================================

        public async Task<IActionResult> Index(
            int? lessonId,
            string? q,
            string? level,
            string? topic,
            string? tab)
        {
            if (lessonId.HasValue && lessonId.Value > 0)
            {
                return RedirectToAction(nameof(Lesson), new { lessonId = lessonId.Value });
            }

            string userId = GetUserId();

            var words = (await _userVocabularyService.GetAllAsync(userId)).ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                words = words.Where(x =>
                    (x.Word?.WordText ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (x.Word?.Definition ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)
                    || (x.Word?.Synonyms ?? "").Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(level))
            {
                words = words.Where(x =>
                    EnglishLevelScale.Normalize(x.Word?.Level) == EnglishLevelScale.Normalize(level)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(topic))
            {
                words = words.Where(x =>
                    string.Equals(x.Word?.TopicSlug, topic, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            words = tab switch
            {
                "new" => words.Where(x => x.Status == "New" || x.CorrectCount == 0).ToList(),
                "learned" => words.Where(x => x.Status == "Mastered" || x.Familiarity >= 4).ToList(),
                "fav" => words.Where(x => x.IsFavorite).ToList(),
                "hard" => words.Where(x => x.IsHard || x.WrongCount >= 2).ToList(),
                _ => words
            };

            ViewBag.TotalWords = await _userVocabularyService.GetTotalWordsAsync(userId);
            ViewBag.MasteredWords = await _userVocabularyService.GetMasteredCountAsync(userId);
            ViewBag.ReviewWords = await _userVocabularyService.GetReviewCountAsync(userId);
            ViewBag.Query = q;
            ViewBag.Level = level;
            ViewBag.Topic = topic;
            ViewBag.Tab = tab;

            return View(words);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int wordId)
        {
            await _userVocabularyService.ToggleFavoriteAsync(GetUserId(), wordId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHard(int wordId)
        {
            await _userVocabularyService.ToggleHardAsync(GetUserId(), wordId);
            return RedirectToAction(nameof(Index));
        }

        // =====================================
        // REVIEW TODAY
        // =====================================

        public async Task<IActionResult> Review()
        {
            string userId = GetUserId();

            var words =
                await _userVocabularyService
                    .GetWordsToReviewAsync(userId);

            ViewBag.DueCount =
                await _userVocabularyService
                    .GetReviewCountAsync(userId);

            ViewBag.ReviewLimit =
                await _userVocabularyService
                    .GetVocabDailyTargetAsync(userId);

            return View(words);
        }

        public async Task<IActionResult> Quiz(string mode = "type")
        {
            mode = NormalizeQuizMode(mode);
            var model = await BuildQuizAsync(mode);

            if (model.Item == null)
            {
                TempData["success"] = "Không còn từ đến hạn để kiểm tra.";
                return RedirectToAction(nameof(Review));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Quiz(string mode, int wordId, string? userAnswer)
        {
            mode = NormalizeQuizMode(mode);
            var userId = GetUserId();
            var item = await _userVocabularyService.GetAsync(userId, wordId);

            if (item?.Word == null)
            {
                return RedirectToAction(nameof(Quiz), new { mode });
            }

            var expected = mode == "choice"
                ? WordMeaningText(item.Word)
                : item.Word.WordText;

            var correct = ExerciseGrading.TextAnswerMatches(expected, userAnswer);

            if (correct)
            {
                await _userVocabularyService.MarkCorrectAsync(userId, wordId);
                await _studyActivity.TrackAsync(userId, points: 2, minutes: 1);
            }
            else
            {
                await _userVocabularyService.MarkWrongAsync(userId, wordId);
            }

            var next = await BuildQuizAsync(mode, skipWordId: wordId);
            next.UserAnswer = userAnswer?.Trim();
            next.IsCorrect = correct;
            next.Item = item;
            next.Choices = mode == "choice"
                ? BuildMeaningChoices(item.Word, await _userVocabularyService.GetAllAsync(userId))
                : next.Choices;

            return View(next);
        }

        // =====================================
        // LEARNING
        // =====================================

        public async Task<IActionResult> Learning()
        {
            string userId = GetUserId();

            var words =
                await _userVocabularyService
                    .GetLearningWordsAsync(userId);

            return View(words);
        }

        // =====================================
        // MASTERED
        // =====================================

        public async Task<IActionResult> Mastered()
        {
            string userId = GetUserId();

            var words =
                await _userVocabularyService
                    .GetMasteredWordsAsync(userId);

            return View(words);
        }

        public async Task<IActionResult> Lesson(int lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null || !lesson.IsPublished)
            {
                return NotFound();
            }

            string userId = GetUserId();

            if (!await _learningAccess.IsEnrolledForLessonAsync(userId, lessonId))
            {
                TempData["error"] = "Hãy đăng ký khóa học trước khi học bài này.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id = lesson.CourseId });
            }

            var words = await _wordService.GetWordsByLessonIdAsync(lessonId);
            var savedIds = (await _userVocabularyService.GetAllAsync(userId))
                .Select(x => x.WordId)
                .ToHashSet();

            await _userLessonService.EnsureMinProgressAsync(userId, lessonId, 40);
            await _userLessonService.MarkVocabDoneAsync(userId, lessonId);
            await _userCourseService.UpdateProgressAsync(userId, lesson.CourseId);
            await _studyActivity.TrackAsync(userId, points: 0, minutes: 5);

            var model = new LessonWordListViewModel
            {
                Lesson = lesson,
                Words = words.Select(word => new LessonWordItemViewModel
                {
                    Word = word,
                    IsSaved = savedIds.Contains(word.Id)
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Calendar()
        {
            string userId = GetUserId();
            var words = await _userVocabularyService.GetAllAsync(userId);
            var grouped = words
                .Where(x => x.NextReviewAt != null)
                .GroupBy(x => DateTime.SpecifyKind(x.NextReviewAt!.Value, DateTimeKind.Utc).ToLocalTime().Date)
                .OrderBy(x => x.Key)
                .ToList();
            return View(grouped);
        }

        public IActionResult Create() => View(new Word());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Word model, string? meaningVi, string? exampleEn, string? exampleVi)
        {
            if (string.IsNullOrWhiteSpace(model.WordText))
            {
                ModelState.AddModelError(nameof(model.WordText), "Nhập từ.");
                return View(model);
            }

            model.WordText = model.WordText.Trim();
            model.Level ??= "A1";
            await _wordService.CreateWordAsync(model);

            if (!string.IsNullOrWhiteSpace(meaningVi))
            {
                _context.wordMeanings.Add(new WordMeaning
                {
                    WordId = model.Id,
                    Language = "vi",
                    Meaning = meaningVi.Trim()
                });
            }

            if (!string.IsNullOrWhiteSpace(exampleEn))
            {
                _context.wordExamples.Add(new WordExample
                {
                    WordId = model.Id,
                    EnglishSentence = exampleEn.Trim(),
                    VietnameseMeaning = exampleVi?.Trim() ?? ""
                });
            }

            await _context.SaveChangesAsync();
            await _userVocabularyService.AddAsync(GetUserId(), model.Id);
            TempData["success"] = "Đã thêm từ cá nhân.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Import() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["error"] = "Chọn file CSV (Word,Meaning,Example,Level,Topic).";
                return View();
            }

            using var reader = new StreamReader(file.OpenReadStream());
            var added = 0;
            var lineNo = 0;
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                lineNo++;
                if (string.IsNullOrWhiteSpace(line) || lineNo == 1 && line.Contains("Word", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var parts = line.Split(',');
                var text = parts.ElementAtOrDefault(0)?.Trim().Trim('"');
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var word = new Word
                {
                    WordText = text,
                    Definition = parts.ElementAtOrDefault(1)?.Trim().Trim('"'),
                    Level = string.IsNullOrWhiteSpace(parts.ElementAtOrDefault(3)) ? "A1" : parts[3].Trim(),
                    TopicSlug = parts.ElementAtOrDefault(4)?.Trim().Trim('"')
                };
                await _wordService.CreateWordAsync(word);
                if (!string.IsNullOrWhiteSpace(word.Definition))
                {
                    _context.wordMeanings.Add(new WordMeaning
                    {
                        WordId = word.Id,
                        Language = "vi",
                        Meaning = word.Definition
                    });
                }

                var example = parts.ElementAtOrDefault(2)?.Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(example))
                {
                    _context.wordExamples.Add(new WordExample
                    {
                        WordId = word.Id,
                        EnglishSentence = example,
                        VietnameseMeaning = word.Definition ?? ""
                    });
                }

                await _context.SaveChangesAsync();
                await _userVocabularyService.AddAsync(GetUserId(), word.Id);
                added++;
            }

            TempData["success"] = $"Đã nhập {added} từ.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Export()
        {
            var words = await _userVocabularyService.GetAllAsync(GetUserId());
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Word,Meaning,IPA,Level,Topic,Status,Correct,Wrong,Favorite,Hard");
            foreach (var item in words)
            {
                var meaning = item.Word?.Meanings.FirstOrDefault()?.Meaning ?? item.Word?.Definition ?? "";
                sb.AppendLine(string.Join(',',
                    Csv(item.Word?.WordText),
                    Csv(meaning),
                    Csv(item.Word?.Pronunciation),
                    Csv(item.Word?.Level),
                    Csv(item.Word?.TopicSlug),
                    Csv(item.Status),
                    item.CorrectCount,
                    item.WrongCount,
                    item.IsFavorite,
                    item.IsHard));
            }

            return File(System.Text.Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "my-vocabulary.csv");
        }

        public async Task<IActionResult> Flashcard(string mode = "due")
        {
            var pool = mode == "quick"
                ? await _userVocabularyService.GetQuickReviewAsync(GetUserId())
                : await _userVocabularyService.GetWordsToReviewAsync(GetUserId());
            ViewBag.Mode = mode;
            ViewBag.DueCount = await _userVocabularyService.GetReviewCountAsync(GetUserId());
            return View(pool.FirstOrDefault());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int wordId, int quality, string mode = "due")
        {
            await _userVocabularyService.RateAsync(GetUserId(), wordId, quality);
            await _studyActivity.TrackAsync(GetUserId(), points: quality >= 3 ? 2 : 0, minutes: 1);
            return RedirectToAction(nameof(Flashcard), new { mode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset(int wordId)
        {
            await _userVocabularyService.ResetAsync(GetUserId(), wordId);
            TempData["success"] = "Đã reset tiến độ từ này.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> QuickReview() =>
            RedirectToAction(nameof(Flashcard), new { mode = "quick" });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShareDeck()
        {
            var words = (await _userVocabularyService.GetAllAsync(GetUserId()))
                .Select(x => x.Word?.WordText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(30);
            _context.ForumPosts.Add(new ForumPost
            {
                ApplicationUserId = GetUserId(),
                Title = "Bộ từ của tôi",
                Body = "Các từ đang học:\n" + string.Join(", ", words),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã chia sẻ bộ từ lên diễn đàn.";
            return RedirectToAction("Index", "Community");
        }

        private static string Csv(string? value)
        {
            var text = (value ?? string.Empty).Replace("\"", "\"\"");
            return $"\"{text}\"";
        }

        // =====================================
        // ADD
        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int wordId, string? returnUrl)
        {
            string userId = GetUserId();

            await _userVocabularyService.AddAsync(userId, wordId);

            var word = await _wordService.GetWordByIdAsync(wordId);

            if (word?.LessonId != null)
            {
                await _userLessonService.EnsureMinProgressAsync(userId, word.LessonId.Value, 40);

                var lesson = await _lessonService.GetLessonByIdAsync(word.LessonId.Value);

                if (lesson != null)
                {
                    await _userCourseService.UpdateProgressAsync(userId, lesson.CourseId);
                }
            }

            TempData["success"] = "Đã thêm từ vào My Vocabulary.";

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================
        // REMOVE
        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int wordId)
        {
            string userId = GetUserId();

            await _userVocabularyService
                .RemoveAsync(userId, wordId);

            TempData["success"] =
                "Đã xóa từ khỏi My Vocabulary.";

            return RedirectToAction(
                nameof(Index));
        }

        // =====================================
        // CORRECT
        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Correct(
            int wordId,
            string? returnUrl)
        {
            string userId = GetUserId();

            await _userVocabularyService
                .MarkCorrectAsync(
                    userId,
                    wordId);

            await _studyActivity.TrackAsync(userId, points: 2, minutes: 1);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(
                nameof(Review));
        }

        // =====================================
        // WRONG
        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Wrong(
            int wordId,
            string? returnUrl)
        {
            string userId = GetUserId();

            await _userVocabularyService
                .MarkWrongAsync(
                    userId,
                    wordId);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(
                nameof(Review));
        }

        // =====================================
        // FAMILIARITY
        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Familiarity(
            int wordId,
            int familiarity)
        {
            string userId = GetUserId();

            await _userVocabularyService
                .UpdateFamiliarityAsync(
                    userId,
                    wordId,
                    familiarity);

            return RedirectToAction(
                nameof(Review));
        }

        // =====================================
        // USER ID
        // =====================================

        private string GetUserId()
        {
            var claim =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);

            if (claim == null ||
                string.IsNullOrWhiteSpace(claim.Value))
            {
                throw new UnauthorizedAccessException();
            }

            return claim.Value;
        }

        private static string NormalizeQuizMode(string? mode)
        {
            return mode switch
            {
                "choice" => "choice",
                "listen" => "listen",
                _ => "type"
            };
        }

        private async Task<VocabQuizViewModel> BuildQuizAsync(string mode, int? skipWordId = null)
        {
            var due = (await _userVocabularyService.GetWordsToReviewAsync(GetUserId()))
                .Where(item => skipWordId == null || item.WordId != skipWordId)
                .ToList();

            var current = due.FirstOrDefault();
            var choices = current?.Word == null
                ? new List<string>()
                : BuildMeaningChoices(current.Word, due);

            return new VocabQuizViewModel
            {
                Mode = mode,
                Item = current,
                Choices = choices,
                Remaining = due.Count
            };
        }

        private static List<string> BuildMeaningChoices(
            Word word,
            IEnumerable<UserVocabulary> pool)
        {
            var answer = WordMeaningText(word);
            var others = pool
                .Where(item => item.Word != null && item.Word.Id != word.Id)
                .Select(item => WordMeaningText(item.Word!))
                .Where(text =>
                    !string.IsNullOrWhiteSpace(text)
                    && !string.Equals(text, answer, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .Take(3)
                .ToList();

            while (others.Count < 3)
            {
                others.Add($"Nghĩa khác {others.Count + 1}");
            }

            return others
                .Append(answer)
                .OrderBy(_ => Random.Shared.Next())
                .ToList();
        }

        private static string WordMeaningText(Word word)
        {
            var meaning = word.Meanings
                .Select(x => x.Meaning)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            return meaning ?? word.Definition ?? word.WordText;
        }
    }
}