using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExerciseController : Controller
    {
        private readonly IExerciseService
            _exerciseService;

        private readonly ILessonService
            _lessonService;

        public ExerciseController(
            IExerciseService exerciseService,
            ILessonService lessonService)
        {
            _exerciseService =
                exerciseService;

            _lessonService =
                lessonService;
        }

        // =========================================
        // INDEX
        // =========================================

        public async Task<IActionResult> Index(
            int? lessonId)
        {
            await LoadLessons();

            if (lessonId.HasValue &&
                lessonId.Value > 0)
            {
                ViewBag.SelectedLessonId =
                    lessonId.Value;
            }

            return View();
        }

        // =========================================
        // GET ALL
        // =========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int? lessonId,
            string? type)
        {
            var exercises =
                await _exerciseService
                    .SearchExerciseAsync(
                        search,
                        lessonId,
                        type);

            return Json(new
            {
                data = exercises
            });
        }

        // =========================================
        // UPSERT GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Upsert(
            int? id,
            int? lessonId)
        {
            await LoadLessons();

            // CREATE
            if (id == null || id == 0)
            {
                var exercise =
                    new Exercise();

                if (lessonId.HasValue &&
                    lessonId.Value > 0)
                {
                    exercise.LessonId =
                        lessonId.Value;
                }

                return View(exercise);
            }

            // EDIT
            var existingExercise =
                await _exerciseService
                    .GetExerciseByIdAsync(
                        id.Value);

            if (existingExercise == null)
                return NotFound();

            return View(existingExercise);
        }

        // =========================================
        // UPSERT POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            Exercise exercise)
        {
            if (!ModelState.IsValid)
            {
                await LoadLessons();

                return View(exercise);
            }

            bool questionExists =
                await _exerciseService
                    .IsQuestionExistsAsync(
                        exercise.LessonId,
                        exercise.Question,
                        exercise.Id);

            if (questionExists)
            {
                ModelState.AddModelError(
                    nameof(exercise.Question),
                    "Question này đã tồn tại trong Lesson.");

                await LoadLessons();

                return View(exercise);
            }

            exercise.Type = ExerciseTypes.Normalize(exercise.Type);

            if (exercise.Id == 0)
            {
                await _exerciseService
                    .CreateExerciseAsync(
                        exercise);

                TempData["success"] =
                    "Thêm Exercise thành công.";
            }
            else
            {
                await _exerciseService
                    .UpdateExerciseAsync(
                        exercise);

                TempData["success"] =
                    "Cập nhật Exercise thành công.";
            }

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================
        // DETAILS
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var exercise =
                await _exerciseService
                    .GetExerciseByIdAsync(id);

            if (exercise == null)
                return NotFound();

            return View(exercise);
        }

        // =========================================
        // DELETE
        // =========================================

        [HttpDelete]
        public async Task<IActionResult> Delete(
            int id)
        {
            bool result =
                await _exerciseService
                    .DeleteExerciseAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy Exercise."
                });
            }

            return Json(new
            {
                success = true,
                message =
                    "Xóa Exercise thành công."
            });
        }

        // =========================================
        // LOAD LESSONS
        // =========================================

        private async Task LoadLessons()
        {
            var lessons =
                await _lessonService
                    .GetAllLessonAsync();

            ViewBag.Lessons =
                new SelectList(
                    lessons,
                    "Id",
                    "Title");
        }
    }
}