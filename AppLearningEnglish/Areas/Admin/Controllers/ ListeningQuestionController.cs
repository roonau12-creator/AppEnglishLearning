using AppLearningEnglish.Business.IService;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ListeningQuestionController : Controller
    {
        private readonly IListeningQuestionService
            _listeningQuestionService;

        private readonly IListeningLessonService
            _listeningLessonService;

        public ListeningQuestionController(
            IListeningQuestionService listeningQuestionService,
            IListeningLessonService listeningLessonService)
        {
            _listeningQuestionService =
                listeningQuestionService;

            _listeningLessonService =
                listeningLessonService;
        }

        // ==========================================
        // INDEX
        // ==========================================

        public async Task<IActionResult> Index(
            int? listeningLessonId)
        {
            await LoadListeningLessons();

            if (listeningLessonId.HasValue &&
                listeningLessonId.Value > 0)
            {
                ViewBag.SelectedListeningLessonId =
                    listeningLessonId.Value;
            }

            return View();
        }

        // ==========================================
        // GET ALL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int? listeningLessonId)
        {
            var questions =
                await _listeningQuestionService
                    .SearchListeningQuestionAsync(
                        search,
                        listeningLessonId);

            var data =
                from item in questions
                select new
                {
                    id = item.Id,

                    listeningLessonId =
                        item.ListeningLessonId,

                    listeningLessonTitle =
                        item.ListeningLesson != null &&
                        item.ListeningLesson.Lesson != null
                            ? item.ListeningLesson.Lesson.Title
                            : "N/A",

                    question =
                        item.Question,

                    answer =
                        item.Answer
                };

            return Json(new
            {
                data = data
            });
        }

        // ==========================================
        // UPSERT GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Upsert(
            int? id,
            int? listeningLessonId)
        {
            await LoadListeningLessons();

            // CREATE

            if (id == null || id == 0)
            {
                var model =
                    new ListeningQuestion();

                if (listeningLessonId.HasValue &&
                    listeningLessonId.Value > 0)
                {
                    model.ListeningLessonId =
                        listeningLessonId.Value;
                }

                return View(model);
            }

            // EDIT

            var existing =
                await _listeningQuestionService
                    .GetListeningQuestionByIdAsync(
                        id.Value);

            if (existing == null)
                return NotFound();

            return View(existing);
        }

        // ==========================================
        // UPSERT POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            ListeningQuestion listeningQuestion)
        {
            bool questionExists =
                await _listeningQuestionService
                    .IsQuestionExistsAsync(
                        listeningQuestion.ListeningLessonId,
                        listeningQuestion.Question,
                        listeningQuestion.Id);

            if (questionExists)
            {
                ModelState.AddModelError(
                    nameof(listeningQuestion.Question),
                    "Question này đã tồn tại trong Listening Lesson.");
            }

            if (!ModelState.IsValid)
            {
                await LoadListeningLessons();

                return View(listeningQuestion);
            }

            if (listeningQuestion.Id == 0)
            {
                await _listeningQuestionService
                    .CreateListeningQuestionAsync(
                        listeningQuestion);

                TempData["success"] =
                    "Thêm Listening Question thành công.";
            }
            else
            {
                await _listeningQuestionService
                    .UpdateListeningQuestionAsync(
                        listeningQuestion);

                TempData["success"] =
                    "Cập nhật Listening Question thành công.";
            }

            return RedirectToAction(
                nameof(Index));
        }

        // ==========================================
        // DETAILS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var question =
                await _listeningQuestionService
                    .GetListeningQuestionByIdAsync(id);

            if (question == null)
                return NotFound();

            return View(question);
        }

        // ==========================================
        // DELETE
        // ==========================================

        [HttpDelete]
        public async Task<IActionResult> Delete(
            int id)
        {
            bool result =
                await _listeningQuestionService
                    .DeleteListeningQuestionAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy Listening Question."
                });
            }

            return Json(new
            {
                success = true,
                message =
                    "Xóa Listening Question thành công."
            });
        }

        // ==========================================
        // LOAD LISTENING LESSONS
        // ==========================================

        private async Task LoadListeningLessons()
        {
            var listeningLessons =
                await _listeningLessonService
                    .GetAllListeningLessonAsync();

            var items =
                from item in listeningLessons
                select new
                {
                    Id = item.Id,

                    Name =
                        item.Lesson != null
                            ? item.Lesson.Title
                            : "Listening #" + item.Id
                };

            ViewBag.ListeningLessons =
                new SelectList(
                    items,
                    "Id",
                    "Name");
        }
    }
}