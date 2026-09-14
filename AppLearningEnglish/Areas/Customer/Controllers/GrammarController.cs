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
    public class GrammarController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly ApplicationDbContext _context;
        private readonly IStudyActivityService _studyActivity;
        private readonly ILearningAccessService _learningAccess;
        private readonly IUserLessonService _userLessonService;
        private readonly IProductHubService _product;

        public GrammarController(
            ILessonService lessonService,
            ApplicationDbContext context,
            IStudyActivityService studyActivity,
            ILearningAccessService learningAccess,
            IUserLessonService userLessonService,
            IProductHubService product)
        {
            _lessonService = lessonService;
            _context = context;
            _studyActivity = studyActivity;
            _learningAccess = learningAccess;
            _userLessonService = userLessonService;
            _product = product;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var userLevel = await _learningAccess.GetUserLevelAsync(userId);
            var topics = await _context.GrammarTopics
                .AsNoTracking()
                .Include(x => x.Items)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var lessons = (await _lessonService.GetAllLessonAsync())
                .Where(x => x.IsPublished && !string.IsNullOrWhiteSpace(x.GrammarNotes))
                .OrderBy(x => x.Course?.Name)
                .ThenBy(x => x.LessonOrder)
                .ToList();

            return View(new GrammarHubViewModel
            {
                UserLevel = userLevel,
                Topics = topics,
                LessonDrills = lessons,
                Progress = await _product.GetGrammarProgressAsync(userId)
            });
        }

        public async Task<IActionResult> Study(int id)
        {
            var topic = await _context.GrammarTopics
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            return topic == null ? NotFound() : View(topic);
        }

        public async Task<IActionResult> TopicDrill(int id)
        {
            var model = await BuildTopicAsync(id);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopicDrill(int id, Dictionary<int, string> answers)
        {
            var model = await BuildTopicAsync(id);

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

            model.CorrectCount = correct;
            var percent = model.Items.Count == 0
                ? 0
                : (int)Math.Round(100d * correct / model.Items.Count);
            await _product.SaveGrammarProgressAsync(GetUserId(), id, percent);

            if (correct > 0)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: correct, minutes: 5);
            }

            if (model.Items.Count > 0 && correct * 2 >= model.Items.Count)
            {
                await _userLessonService.MarkGrammarForTopicAsync(GetUserId(), id);
            }

            return View(model);
        }

        public async Task<IActionResult> Drill(int id)
        {
            var model = await BuildLessonAsync(id);
            return model == null ? NotFound() : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Drill(int id, Dictionary<int, string> answers)
        {
            var model = await BuildLessonAsync(id);

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

            model.CorrectCount = correct;

            if (correct > 0)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: correct, minutes: 4);
            }

            return View(model);
        }

        public IActionResult Conjugate() => View("Practice", BuildConjugate());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Conjugate(Dictionary<int, string> answers)
        {
            var model = GradePractice(BuildConjugate(), answers);
            if ((model.CorrectCount ?? 0) > 0)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: model.CorrectCount ?? 0, minutes: 3);
            }

            return View("Practice", model);
        }

        public IActionResult Rewrite() => View("Practice", BuildRewrite());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rewrite(Dictionary<int, string> answers)
        {
            var model = GradePractice(BuildRewrite(), answers);
            if ((model.CorrectCount ?? 0) > 0)
            {
                await _studyActivity.TrackAsync(GetUserId(), points: model.CorrectCount ?? 0, minutes: 3);
            }

            return View("Practice", model);
        }

        private static GrammarPracticeViewModel BuildConjugate()
        {
            return new GrammarPracticeViewModel
            {
                Title = "Chia động từ",
                Kind = "conjugate",
                Items =
                [
                    new GrammarDrillItem { QuestionId = 1, QuestionText = "She ___ (go) to school every day.", Answer = "goes", Explanation = "Present simple, ngôi 3 số ít thêm -es." },
                    new GrammarDrillItem { QuestionId = 2, QuestionText = "They ___ (be) at home now.", Answer = "are", Explanation = "be → are với they." },
                    new GrammarDrillItem { QuestionId = 3, QuestionText = "I ___ (have) breakfast at 7.", Answer = "have", Explanation = "I + have." },
                    new GrammarDrillItem { QuestionId = 4, QuestionText = "He ___ (study) English last night.", Answer = "studied", Explanation = "Past simple: study → studied." },
                    new GrammarDrillItem { QuestionId = 5, QuestionText = "We ___ (not / like) coffee.", Answer = "do not like", Explanation = "Phủ định present simple: do not + V." }
                ]
            };
        }

        private static GrammarPracticeViewModel BuildRewrite()
        {
            return new GrammarPracticeViewModel
            {
                Title = "Viết lại câu",
                Kind = "rewrite",
                Items =
                [
                    new GrammarDrillItem { QuestionId = 1, QuestionText = "She is a teacher. (câu hỏi yes/no)", Answer = "Is she a teacher?", Explanation = "Đảo be lên đầu." },
                    new GrammarDrillItem { QuestionId = 2, QuestionText = "I can swim. (phủ định)", Answer = "I cannot swim.", Explanation = "can → cannot / can't." },
                    new GrammarDrillItem { QuestionId = 3, QuestionText = "They play football. (hiện tại tiếp diễn)", Answer = "They are playing football.", Explanation = "be + V-ing." },
                    new GrammarDrillItem { QuestionId = 4, QuestionText = "He goes to work. (quá khứ)", Answer = "He went to work.", Explanation = "go → went." }
                ]
            };
        }

        private static GrammarPracticeViewModel GradePractice(
            GrammarPracticeViewModel model,
            Dictionary<int, string> answers)
        {
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

            model.CorrectCount = correct;
            return model;
        }

        private async Task<GrammarTopicViewModel?> BuildTopicAsync(int id)
        {
            var topic = await _context.GrammarTopics
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (topic == null)
            {
                return null;
            }

            return new GrammarTopicViewModel
            {
                Topic = topic,
                Items = topic.Items.Select(item => new GrammarDrillItem
                {
                    QuestionId = item.Id,
                    QuestionText = item.QuestionText,
                    Explanation = item.Explanation,
                    Options = (item.Options ?? string.Empty)
                        .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .ToList(),
                    Answer = item.Answer
                }).ToList()
            };
        }

        private async Task<GrammarDrillViewModel?> BuildLessonAsync(int lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null || !lesson.IsPublished)
            {
                return null;
            }

            var questions = await _context.questions
                .Include(x => x.Exercise)
                .Where(x => x.Exercise != null && x.Exercise.LessonId == lessonId)
                .OrderBy(x => x.Id)
                .Take(12)
                .ToListAsync();

            var items = new List<GrammarDrillItem>();

            foreach (var question in questions)
            {
                var answers = await _context.answers
                    .Where(x => x.QuestionId == question.Id)
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                var correct = answers.FirstOrDefault(x => x.IsCorrect)?.AnswerText
                    ?? answers.FirstOrDefault()?.AnswerText;

                if (string.IsNullOrWhiteSpace(correct))
                {
                    continue;
                }

                items.Add(new GrammarDrillItem
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    Options = answers.Select(x => x.AnswerText).ToList(),
                    Answer = correct
                });
            }

            return new GrammarDrillViewModel
            {
                Lesson = lesson,
                Items = items
            };
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
