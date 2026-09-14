using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Business;
using AppLearningEnglish.Business.Services.IServices;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
     [Area("Customer")]
    [Authorize]
    public class ExerciseController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IExerciseAttemptService _attemptService;
        private readonly IUserLessonService _userLessonService;
        private readonly IUserCourseService _userCourseService;
        private readonly IStudyActivityService _studyActivity;
        private readonly ILearningAccessService _learningAccess;
        private readonly ILessonService _lessonService;

        public ExerciseController(
            ApplicationDbContext context,
            IExerciseAttemptService attemptService,
            IUserLessonService userLessonService,
            IUserCourseService userCourseService,
            IStudyActivityService studyActivity,
            ILearningAccessService learningAccess,
            ILessonService lessonService)
        {
            _context = context;
            _attemptService = attemptService;
            _userLessonService = userLessonService;
            _userCourseService = userCourseService;
            _studyActivity = studyActivity;
            _learningAccess = learningAccess;
            _lessonService = lessonService;
        }

        // ==========================================
        // INDEX
        // ==========================================

        public async Task<IActionResult> Index(
            int lessonId)
        {
            string userId = GetUserId();
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null || !lesson.IsPublished)
            {
                return NotFound();
            }

            if (!await _learningAccess.IsEnrolledForLessonAsync(userId, lessonId))
            {
                TempData["error"] = "Hãy đăng ký khóa học trước khi làm bài tập.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id = lesson.CourseId });
            }

            var query =
                from exercise in _context.exercises
                where exercise.LessonId == lessonId
                orderby exercise.ExerciseOrder
                select exercise;

            var exercises =
                await query.ToListAsync();

            ViewBag.LessonId = lessonId;

            return View(exercises);
        }

        // ==========================================
        // START EXERCISE
        // ==========================================

        public async Task<IActionResult> Start(
            int id)
        {
            var exercise =
                await _context.exercises
                    .FindAsync(id);

            if (exercise == null)
            {
                return NotFound();
            }

            var questionQuery =
                from question in _context.questions
                where question.ExerciseId == id
                select question;

            int totalQuestions =
                await questionQuery.CountAsync();

            if (totalQuestions == 0)
            {
                TempData["error"] =
                    "Exercise chưa có câu hỏi.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        lessonId = exercise.LessonId
                    });
            }

            string userId = GetUserId();

            if (!await _learningAccess.IsEnrolledForLessonAsync(userId, exercise.LessonId))
            {
                var lesson = await _lessonService.GetLessonByIdAsync(exercise.LessonId);
                TempData["error"] = "Hãy đăng ký khóa học trước khi làm bài tập.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id = lesson?.CourseId });
            }

            var attempt =
                await _attemptService.StartAsync(
                    userId,
                    id,
                    totalQuestions);

            return RedirectToAction(
                nameof(DoExercise),
                new
                {
                    attemptId = attempt.Id
                });
        }

        // ==========================================
        // DO EXERCISE
        // ==========================================

        public async Task<IActionResult> DoExercise(
            int attemptId)
        {
            string userId = GetUserId();

            var attempt =
                await _attemptService.GetByIdAsync(
                    attemptId,
                    userId);

            if (attempt == null)
            {
                return NotFound();
            }

            if (attempt.CompletedAt != null)
            {
                return RedirectToAction(
                    nameof(Result),
                    new
                    {
                        attemptId = attempt.Id
                    });
            }

            var questionQuery =
                from question in _context.questions
                where question.ExerciseId ==
                      attempt.ExerciseId
                orderby question.Id
                select question;

            var questions =
                await questionQuery.ToListAsync();

            var renderMode = ExerciseTypes.GetRenderMode(attempt.Exercise?.Type);

            var model =
                new DoExerciseViewModel
                {
                    AttemptId = attempt.Id,
                    ExerciseId = attempt.ExerciseId,
                    ExerciseName =
                        attempt.Exercise?.Question
                        ?? "Exercise",
                    ExerciseType =
                        ExerciseTypes.Normalize(attempt.Exercise?.Type),
                    Passage = renderMode == ExerciseRenderMode.PassageChoice
                        ? attempt.Exercise?.Lesson?.Content
                        : null
                };

            foreach (var question in questions)
            {
                var answerQuery =
                    from answer in _context.answers
                    where answer.QuestionId ==
                          question.Id
                    orderby answer.Id
                    select answer;

                var answers =
                    await answerQuery.ToListAsync();

                var questionModel =
                    new ExerciseQuestionViewModel
                    {
                        QuestionId = question.Id,
                        QuestionText =
                            question.QuestionText
                    };

                foreach (var answer in answers)
                {
                    questionModel.Answers.Add(
                        new ExerciseAnswerViewModel
                        {
                            AnswerId = answer.Id,
                            AnswerText =
                                answer.AnswerText
                        });
                }

                model.Questions.Add(
                    questionModel);
            }

            if (renderMode == ExerciseRenderMode.Matching)
            {
                // Dạng nối cặp: gom đáp án đúng của mọi câu thành một kho
                // rồi xếp cố định theo Id để thứ tự không tiết lộ cặp đúng.
                var pool = await _context.answers
                    .Where(x => questions.Select(q => q.Id).Contains(x.QuestionId) &&
                                x.IsCorrect)
                    .OrderBy(x => x.AnswerText)
                    .Select(x => new ExerciseAnswerViewModel
                    {
                        AnswerId = x.Id,
                        AnswerText = x.AnswerText
                    })
                    .ToListAsync();

                model.MatchingPool = pool;
            }

            return View(model);
        }

        // ==========================================
        // SUBMIT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(
            SubmitExerciseViewModel model)
        {
            string userId = GetUserId();

            var attempt =
                await _attemptService.GetByIdAsync(
                    model.AttemptId,
                    userId);

            if (attempt == null)
            {
                return NotFound();
            }

            if (attempt.CompletedAt != null)
            {
                return RedirectToAction(
                    nameof(Result),
                    new
                    {
                        attemptId = attempt.Id
                    });
            }

            int correctAnswers = 0;

            foreach (var answer in model.Answers)
            {
                var query =
                    from item in _context.answers
                    where item.Id == answer.Value
                          && item.QuestionId ==
                             answer.Key
                    select item;

                var selectedAnswer =
                    await query.FirstOrDefaultAsync();

                if (selectedAnswer != null &&
                    selectedAnswer.IsCorrect)
                {
                    correctAnswers++;
                }
            }

            foreach (var typed in model.TextAnswers)
            {
                var accepted = await _context.answers
                    .Where(x => x.QuestionId == typed.Key && x.IsCorrect)
                    .Select(x => x.AnswerText)
                    .ToListAsync();

                if (accepted.Any(x => TextAnswerMatches(x, typed.Value)))
                {
                    correctAnswers++;
                }
            }

            await _attemptService.SubmitAsync(
                model.AttemptId,
                userId,
                correctAnswers,
                System.Text.Json.JsonSerializer.Serialize(model.Answers),
                System.Text.Json.JsonSerializer.Serialize(model.TextAnswers));

            var exercise = await _context.exercises.FindAsync(attempt.ExerciseId);

            if (exercise != null)
            {
                await _userLessonService.EnsureMinProgressAsync(userId, exercise.LessonId, 90);
                await _userLessonService.MarkExerciseDoneAsync(userId, exercise.LessonId);

                var lesson = await _context.Lessons.FindAsync(exercise.LessonId);

                if (lesson != null)
                {
                    await _userCourseService.UpdateProgressAsync(userId, lesson.CourseId);
                }

                var previous = await _attemptService.GetByExerciseAsync(userId, attempt.ExerciseId);
                var firstComplete = previous.Count(x => x.CompletedAt != null && x.Id != attempt.Id) == 0;
                var points = firstComplete
                    ? 10 + (correctAnswers * 5)
                    : Math.Max(2, correctAnswers);

                await _studyActivity.TrackAsync(userId, points, minutes: 10);
                TempData["success"] = $"Bạn đã hoàn thành Exercise và nhận {points} điểm.";
            }
            else
            {
                TempData["success"] = "Bạn đã hoàn thành Exercise.";
            }

            return RedirectToAction(
                nameof(Result),
                new
                {
                    attemptId = model.AttemptId
                });
        }

        // ==========================================
        // RESULT
        // ==========================================

        public async Task<IActionResult> Result(
            int attemptId)
        {
            string userId = GetUserId();

            var attempt =
                await _attemptService.GetByIdAsync(
                    attemptId,
                    userId);

            if (attempt == null)
            {
                return NotFound();
            }

            var questions = await _context.questions
                .Where(x => x.ExerciseId == attempt.ExerciseId)
                .OrderBy(x => x.Id)
                .ToListAsync();

            var selected = new Dictionary<int, int>();
            if (!string.IsNullOrWhiteSpace(attempt.SelectedAnswersJson))
            {
                selected = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(
                    attempt.SelectedAnswersJson) ?? new Dictionary<int, int>();
            }

            var typedAnswers = new Dictionary<int, string>();
            if (!string.IsNullOrWhiteSpace(attempt.TextAnswersJson))
            {
                typedAnswers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>(
                    attempt.TextAnswersJson) ?? new Dictionary<int, string>();
            }

            var isFillBlank = string.Equals(
                attempt.Exercise?.Type,
                ExerciseTypes.FillBlank,
                StringComparison.OrdinalIgnoreCase);

            var review = new ExerciseResultReviewViewModel
            {
                Attempt = attempt,
                LessonId = attempt.Exercise?.LessonId
            };

            foreach (var question in questions)
            {
                var answers = await _context.answers
                    .Where(x => x.QuestionId == question.Id)
                    .OrderBy(x => x.Id)
                    .ToListAsync();

                selected.TryGetValue(question.Id, out var selectedId);
                typedAnswers.TryGetValue(question.Id, out var userText);

                var correctText = answers
                    .FirstOrDefault(x => x.IsCorrect)?.AnswerText;

                review.Questions.Add(new ExerciseResultQuestionViewModel
                {
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    IsFillBlank = isFillBlank,
                    UserText = userText,
                    CorrectText = correctText,
                    IsCorrect = isFillBlank
                        ? answers.Any(x => x.IsCorrect &&
                                           TextAnswerMatches(x.AnswerText, userText))
                        : answers.Any(x => x.IsCorrect && x.Id == selectedId),
                    Answers = answers.Select(answer => new ExerciseResultAnswerViewModel
                    {
                        AnswerText = answer.AnswerText,
                        IsCorrect = answer.IsCorrect,
                        IsSelected = answer.Id == selectedId
                    }).ToList()
                });
            }

            return View(review);
        }

        // ==========================================
        // HISTORY
        // ==========================================

        public async Task<IActionResult> History(
            int exerciseId)
        {
            string userId = GetUserId();

            var attempts =
                await _attemptService
                    .GetByExerciseAsync(
                        userId,
                        exerciseId);

            ViewBag.ExerciseId =
                exerciseId;

            return View(attempts);
        }

        // ==========================================
        // ALL HISTORY
        // ==========================================

        public async Task<IActionResult> MyHistory()
        {
            string userId = GetUserId();

            var attempts =
                await _attemptService
                    .GetAllAsync(userId);

            return View(attempts);
        }

        // ==========================================
        // GET USER ID
        // ==========================================

        private static bool TextAnswerMatches(
            string? expected,
            string? actual)
        {
            return ExerciseGrading.TextAnswerMatches(expected, actual);
        }

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
    }
    
}