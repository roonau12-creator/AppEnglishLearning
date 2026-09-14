using AppLearningEnglish.Business;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ReadingController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly IWordService _wordService;
        private readonly IStudyActivityService _studyActivity;
        private readonly ILearningAccessService _learningAccess;
        private readonly IUserLessonService _userLessonService;
        private readonly ApplicationDbContext _context;
        private readonly IUserVocabularyService _userVocabulary;

        public ReadingController(
            ILessonService lessonService,
            IWordService wordService,
            IStudyActivityService studyActivity,
            ILearningAccessService learningAccess,
            IUserLessonService userLessonService,
            ApplicationDbContext context,
            IUserVocabularyService userVocabulary)
        {
            _lessonService = lessonService;
            _wordService = wordService;
            _studyActivity = studyActivity;
            _learningAccess = learningAccess;
            _userLessonService = userLessonService;
            _context = context;
            _userVocabulary = userVocabulary;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var userLevel = await _learningAccess.GetUserLevelAsync(userId);
            var passages = await _context.ReadingPassages
                .AsNoTracking()
                .Include(x => x.Questions)
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Id)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(userLevel))
            {
                passages = passages
                    .Where(x => !EnglishLevelScale.IsTooHard(userLevel, x.Level))
                    .ToList();
            }

            var lessons = (await _lessonService.GetAllLessonAsync())
                .Where(x => x.IsPublished && !string.IsNullOrWhiteSpace(x.Content))
                .OrderBy(x => x.Course?.Name)
                .ThenBy(x => x.LessonOrder)
                .ToList();

            return View(new ReadingHubViewModel
            {
                Passages = passages,
                LessonReadings = lessons
            });
        }

        public async Task<IActionResult> Passage(int id)
        {
            var model = await BuildPassageAsync(id);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Passage(int id, Dictionary<int, string> answers)
        {
            var model = await BuildPassageAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            var userLevel = await _learningAccess.GetUserLevelAsync(GetUserId());
            if (EnglishLevelScale.IsTooHard(userLevel, model.Passage.Level))
            {
                TempData["error"] = "Bài đọc này cao hơn trình độ hiện tại. Hãy làm bài xếp lớp hoặc chọn bài dễ hơn.";
                return RedirectToAction(nameof(Index));
            }

            var correct = 0;

            foreach (var question in model.Questions)
            {
                answers.TryGetValue(question.QuestionId, out var chosen);
                question.UserAnswer = chosen;
                question.IsCorrect = ExerciseGrading.TextAnswerMatches(question.Answer, chosen);

                if (question.IsCorrect == true)
                {
                    correct++;
                }
            }

            model.Score = correct;
            model.Total = model.Questions.Count;

            if (correct > 0)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: correct * 2, minutes: 8);
            }

            if (model.Total > 0 && correct * 2 >= model.Total)
            {
                await _userLessonService.MarkReadingForPassageAsync(GetUserId(), id);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWord(int passageId, string wordText)
        {
            var text = (wordText ?? string.Empty).Trim();
            if (text.Length < 2)
            {
                TempData["error"] = "Chọn một từ để lưu.";
                return RedirectToAction(nameof(Passage), new { id = passageId });
            }

            var word = await _context.words.FirstOrDefaultAsync(x =>
                x.WordText.ToLower() == text.ToLower());

            if (word == null)
            {
                word = new AppLearningEnglish.Models.Word
                {
                    WordText = text,
                    Definition = "Từ lưu từ bài đọc",
                    TopicSlug = "school"
                };
                _context.words.Add(word);
                await _context.SaveChangesAsync();
            }

            await _userVocabulary.AddAsync(GetUserId(), word.Id);
            TempData["success"] = $"Đã lưu “{word.WordText}” vào sổ từ.";
            return RedirectToAction(nameof(Passage), new { id = passageId });
        }

        public async Task<IActionResult> Play(int id)
        {
            var model = await BuildLessonAsync(id);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Play(int id, Dictionary<int, string> answers)
        {
            var model = await BuildLessonAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            var correct = 0;

            foreach (var question in model.Questions)
            {
                answers.TryGetValue(question.WordId, out var chosen);
                question.UserAnswer = chosen;
                question.IsCorrect = string.Equals(
                    chosen,
                    question.Answer,
                    StringComparison.OrdinalIgnoreCase);

                if (question.IsCorrect == true)
                {
                    correct++;
                }
            }

            model.Score = correct;
            model.Total = model.Questions.Count;

            if (correct > 0)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: correct, minutes: 5);
            }

            return View(model);
        }

        private async Task<ReadingPassagePlayViewModel?> BuildPassageAsync(int id)
        {
            var passage = await _context.ReadingPassages
                .AsNoTracking()
                .Include(x => x.Questions)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (passage == null)
            {
                return null;
            }

            var learned = (await _userVocabulary.GetAllAsync(GetUserId()))
                .Select(x => x.Word?.WordText)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new ReadingPassagePlayViewModel
            {
                Passage = passage,
                LearnedWords = learned,
                Questions = passage.Questions
                    .OrderBy(x => x.Id)
                    .Select(question => new ReadingPassageItem
                    {
                        QuestionId = question.Id,
                        Type = question.Type,
                        Prompt = question.Prompt,
                        Options = (question.Options ?? string.Empty)
                            .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .ToList(),
                        Answer = question.Answer,
                        Explanation = question.Explanation
                    })
                    .ToList()
            };
        }

        private async Task<ReadingPlayViewModel?> BuildLessonAsync(int lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null || !lesson.IsPublished || string.IsNullOrWhiteSpace(lesson.Content))
            {
                return null;
            }

            var words = (await _wordService.GetWordsByLessonIdAsync(lessonId)).ToList();
            var pool = words
                .Where(word =>
                    word.Meanings.Any(meaning => !string.IsNullOrWhiteSpace(meaning.Meaning))
                    || !string.IsNullOrWhiteSpace(word.Definition))
                .Take(6)
                .ToList();

            var distractors = words
                .Select(WordMeaningText)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Distinct()
                .ToList();

            if (distractors.Count < 3)
            {
                distractors.AddRange(["a person", "a place", "an action", "a feeling"]);
            }

            var random = new Random(lessonId);
            var questions = pool.Select(word =>
            {
                var answer = WordMeaningText(word);
                var options = distractors
                    .Where(text => !string.Equals(text, answer, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(_ => random.Next())
                    .Take(3)
                    .Append(answer)
                    .OrderBy(_ => random.Next())
                    .ToList();

                return new ReadingQuizItem
                {
                    WordId = word.Id,
                    Prompt = $"Trong đoạn, \"{word.WordText}\" gần nghĩa nào nhất?",
                    Answer = answer,
                    Options = options
                };
            }).ToList();

            return new ReadingPlayViewModel
            {
                Lesson = lesson,
                Questions = questions
            };
        }

        private static string WordMeaningText(AppLearningEnglish.Models.Word word)
        {
            var meaning = word.Meanings
                .Select(x => x.Meaning)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            return meaning ?? word.Definition ?? word.WordText;
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
