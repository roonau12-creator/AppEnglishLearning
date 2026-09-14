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
    public class LessonController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly IUserLessonService _userLessonService;
        private readonly IUserCourseService _userCourseService;
        private readonly IWordService _wordService;
        private readonly IExerciseService _exerciseService;
        private readonly IListeningLessonService _listeningLessonService;
        private readonly IStudyActivityService _studyActivity;
        private readonly ILearningAccessService _learningAccess;
        private readonly ILearnerWorkspaceService _workspace;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonController(
            ILessonService lessonService,
            IUserLessonService userLessonService,
            IUserCourseService userCourseService,
            IWordService wordService,
            IExerciseService exerciseService,
            IListeningLessonService listeningLessonService,
            IStudyActivityService studyActivity,
            ILearningAccessService learningAccess,
            ILearnerWorkspaceService workspace,
            UserManager<ApplicationUser> userManager)
        {
            _lessonService = lessonService;
            _userLessonService = userLessonService;
            _userCourseService = userCourseService;
            _wordService = wordService;
            _exerciseService = exerciseService;
            _listeningLessonService = listeningLessonService;
            _studyActivity = studyActivity;
            _learningAccess = learningAccess;
            _workspace = workspace;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(int id)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(id);

            if (lesson == null || !lesson.IsPublished)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (!await _learningAccess.IsEnrolledForLessonAsync(userId, id))
            {
                TempData["error"] = "Hãy đăng ký khóa học trước khi học bài này.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id = lesson.CourseId });
            }

            if (!await _learningAccess.CanAccessCourseLevelAsync(userId, lesson.Course?.Level))
            {
                TempData["error"] = "Bài này thuộc khóa cao hơn trình độ hiện tại.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id = lesson.CourseId });
            }

            await _userLessonService.StartLessonAsync(userId, id);
            await _userLessonService.EnsureMinProgressAsync(userId, id, 10);

            var progress = await _userLessonService.GetUserLessonAsync(userId, id);
            var words = await _wordService.GetWordsByLessonIdAsync(id);
            var listening = await _listeningLessonService.GetByLessonIdAsync(id);
            var exercises = await _exerciseService.GetByLessonIdAsync(id);
            var hasVocab = words.Any();
            var hasListening = listening != null;
            var hasExercise = exercises.Any();
            var hasReading = lesson.ReadingPassageId.HasValue;
            var hasSpeaking = hasVocab;
            var hasWriting = lesson.WritingPromptId.HasValue;
            var hasGrammar = lesson.GrammarTopicId.HasValue;
            var missing = LessonCompletionHelper.GetMissingSteps(
                progress, hasVocab, hasListening, hasExercise,
                hasReading, hasSpeaking, hasWriting, hasGrammar);

            var note = await _workspace.GetLessonNoteAsync(userId, id);

            return View(new LessonDetailsViewModel
            {
                Lesson = lesson,
                Progress = progress,
                WordCount = words.Count(),
                HasListening = hasListening,
                HasExercise = hasExercise,
                YoutubeEmbedUrl = MediaUrlHelper.ToYoutubeEmbed(lesson.VideoUrl),
                VocabDone = progress?.VocabDone == true,
                ListeningDone = progress?.ListeningDone == true,
                ExerciseDone = progress?.ExerciseDone == true,
                ReadingDone = progress?.ReadingDone == true,
                SpeakingDone = progress?.SpeakingDone == true,
                WritingDone = progress?.WritingDone == true,
                GrammarDone = progress?.GrammarDone == true,
                HasReading = hasReading,
                HasSpeaking = hasSpeaking,
                HasWriting = hasWriting,
                HasGrammar = hasGrammar,
                CanComplete = missing.Count == 0,
                MissingSteps = missing,
                NoteBody = note?.Body,
                IsBookmarked = await _workspace.IsBookmarkedAsync(userId, id)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNote(int lessonId, string? body)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await _workspace.SaveLessonNoteAsync(userId, lessonId, body ?? string.Empty);
            TempData["success"] = "Đã lưu ghi chú.";
            return RedirectToAction(nameof(Details), new { id = lessonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBookmark(int lessonId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await _workspace.ToggleBookmarkAsync(userId, lessonId);
            return RedirectToAction(nameof(Details), new { id = lessonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(int lessonId, decimal progress)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            await _userLessonService.UpdateProgressAsync(userId, lessonId, progress);
            await _userCourseService.UpdateProgressAsync(userId, lesson.CourseId);

            return RedirectToAction(nameof(Details), new { id = lessonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int lessonId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            if (!await _learningAccess.IsEnrolledForLessonAsync(userId, lessonId))
            {
                TempData["error"] = "Hãy đăng ký khóa học trước khi học bài này.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id = lesson.CourseId });
            }

            var words = await _wordService.GetWordsByLessonIdAsync(lessonId);
            var listening = await _listeningLessonService.GetByLessonIdAsync(lessonId);
            var exercises = await _exerciseService.GetByLessonIdAsync(lessonId);
            var progress = await _userLessonService.GetUserLessonAsync(userId, lessonId);
            var missing = LessonCompletionHelper.GetMissingSteps(
                progress,
                words.Any(),
                listening != null,
                exercises.Any());

            if (missing.Count > 0)
            {
                TempData["error"] = "Hãy hoàn thành Vocabulary, Listening và Exercise trước khi kết thúc bài.";
                return RedirectToAction(nameof(Details), new { id = lessonId });
            }

            var user = await _userManager.FindByIdAsync(userId);
            var levelBefore = user?.EnglishLevel;
            var newlyCompleted = await _userLessonService.CompleteLessonAsync(userId, lessonId);
            await _userCourseService.UpdateProgressAsync(userId, lesson.CourseId);
            user = await _userManager.FindByIdAsync(userId);

            if (newlyCompleted)
            {
                await _studyActivity.TrackAsync(userId, points: 20, minutes: 5);
                var message = "Chúc mừng! Bạn đã hoàn thành bài học và nhận 20 điểm.";
                if (!string.Equals(levelBefore, user?.EnglishLevel, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(user?.EnglishLevel))
                {
                    message += $" Trình độ mới: {user.EnglishLevel}.";
                }

                TempData["success"] = message;
            }
            else
            {
                TempData["success"] = "Bạn đã hoàn thành bài học này trước đó.";
            }

            return RedirectToAction(nameof(Details), new { id = lessonId });
        }
    }
}
