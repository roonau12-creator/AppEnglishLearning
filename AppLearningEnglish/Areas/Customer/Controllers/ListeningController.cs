using AppLearningEnglish.Business;
using AppLearningEnglish.Business.IService;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ListeningController : Controller
    {
        private readonly IListeningLessonService _listeningLessonService;
        private readonly IListeningQuestionService _listeningQuestionService;
        private readonly ILessonService _lessonService;
        private readonly IUserLessonService _userLessonService;
        private readonly IUserCourseService _userCourseService;
        private readonly IStudyActivityService _studyActivity;
        private readonly ILearningAccessService _learningAccess;
        private readonly UserManager<ApplicationUser> _userManager;

        public ListeningController(
            IListeningLessonService listeningLessonService,
            IListeningQuestionService listeningQuestionService,
            ILessonService lessonService,
            IUserLessonService userLessonService,
            IUserCourseService userCourseService,
            IStudyActivityService studyActivity,
            ILearningAccessService learningAccess,
            UserManager<ApplicationUser> userManager)
        {
            _listeningLessonService = listeningLessonService;
            _listeningQuestionService = listeningQuestionService;
            _lessonService = lessonService;
            _userLessonService = userLessonService;
            _userCourseService = userCourseService;
            _studyActivity = studyActivity;
            _learningAccess = learningAccess;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? lessonId)
        {
            if (lessonId.HasValue && lessonId.Value > 0)
            {
                return RedirectToAction(nameof(Play), new { lessonId = lessonId.Value });
            }

            var listenings = await _listeningLessonService.GetAllListeningLessonAsync();
            var published = listenings
                .Where(x => x.Lesson != null && x.Lesson.IsPublished)
                .ToList();

            return View(published);
        }

        public async Task<IActionResult> Play(int lessonId)
        {
            var listening = await _listeningLessonService.GetByLessonIdAsync(lessonId);

            if (listening == null)
            {
                TempData["error"] = "Bài học này chưa có phần Listening.";
                return RedirectToAction("Details", "Lesson", new { area = "Customer", id = lessonId });
            }

            var lesson = listening.Lesson ?? await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null || !lesson.IsPublished)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId) &&
                !await _learningAccess.IsEnrolledForLessonAsync(userId, lessonId))
            {
                TempData["error"] = "Hãy đăng ký khóa học trước khi luyện nghe.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id = lesson.CourseId });
            }

            var questions = await _listeningQuestionService.GetByListeningLessonIdAsync(listening.Id);

            return View(new ListeningPlayViewModel
            {
                Lesson = lesson,
                ListeningLesson = listening,
                Questions = questions.ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int lessonId, Dictionary<int, string>? answers)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var listening = await _listeningLessonService.GetByLessonIdAsync(lessonId);

            if (listening == null)
            {
                return NotFound();
            }

            var lesson = listening.Lesson ?? await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            if (!await _learningAccess.IsEnrolledForLessonAsync(userId, lessonId))
            {
                TempData["error"] = "Hãy đăng ký khóa học trước khi luyện nghe.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id = lesson.CourseId });
            }

            var questions = (await _listeningQuestionService.GetByListeningLessonIdAsync(listening.Id)).ToList();
            answers ??= new Dictionary<int, string>();

            var results = new List<ListeningResultItemViewModel>();
            var correctCount = 0;

            foreach (var question in questions)
            {
                answers.TryGetValue(question.Id, out var userAnswer);
                userAnswer ??= string.Empty;

                var isCorrect = IsListeningAnswerCorrect(
                    question.QuestionType,
                    question.Answer,
                    userAnswer);
                var accuracy = TextSimilarity.Percent(question.Answer, userAnswer);

                if (isCorrect)
                {
                    correctCount++;
                }

                results.Add(new ListeningResultItemViewModel
                {
                    QuestionId = question.Id,
                    Question = question.Question,
                    ExpectedAnswer = question.Answer,
                    UserAnswer = userAnswer.Trim(),
                    IsCorrect = isCorrect,
                    AccuracyPercent =
                        question.QuestionType == ListeningQuestionTypes.Dictation
                            ? accuracy
                            : null
                });
            }

            var progress = await _userLessonService.GetUserLessonAsync(userId, lessonId);
            var firstTime = progress == null || !progress.ListeningDone;
            var points = firstTime
                ? 10 + (correctCount * 5)
                : Math.Max(2, correctCount);

            await _userLessonService.EnsureMinProgressAsync(userId, lessonId, 70);
            await _userLessonService.MarkListeningDoneAsync(userId, lessonId);
            await _userCourseService.UpdateProgressAsync(userId, lesson.CourseId);
            await _studyActivity.TrackAsync(userId, points, minutes: 10);

            return View("Result", new ListeningResultViewModel
            {
                Lesson = lesson,
                ListeningLesson = listening,
                Results = results,
                CorrectCount = correctCount,
                TotalQuestions = questions.Count,
                PointsEarned = points
            });
        }

        private static bool IsListeningAnswerCorrect(
            string questionType,
            string expected,
            string actual)
        {
            if (questionType == ListeningQuestionTypes.Dictation ||
                questionType == ListeningQuestionTypes.Sentence ||
                questionType == ListeningQuestionTypes.TranscriptGap)
            {
                return ExerciseGrading.TextAnswerMatches(expected, actual);
            }

            var expectedNorm = Normalize(expected);
            var actualNorm = Normalize(actual);

            if (string.IsNullOrWhiteSpace(expectedNorm) || string.IsNullOrWhiteSpace(actualNorm))
            {
                return false;
            }

            return actualNorm == expectedNorm
                || actualNorm.Contains(expectedNorm)
                || expectedNorm.Contains(actualNorm);
        }

        private static string Normalize(string value)
        {
            return string.Join(
                " ",
                value.Trim().ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
