using AppLearningEnglish.Business.IService;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AnswerController : Controller
    {
        private readonly IAnswerService
            _answerService;

        private readonly IQuestionService
            _questionService;

        public AnswerController(
            IAnswerService answerService,
            IQuestionService questionService)
        {
            _answerService =
                answerService;

            _questionService =
                questionService;
        }

        // ==========================================
        // INDEX
        // ==========================================

        public async Task<IActionResult> Index(
            int? questionId)
        {
            await LoadQuestions();

            if (questionId.HasValue &&
                questionId.Value > 0)
            {
                ViewBag.SelectedQuestionId =
                    questionId.Value;
            }

            return View();
        }

        // ==========================================
        // GET ALL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int? questionId)
        {
            var answers =
                await _answerService
                    .SearchAnswerAsync(
                        search,
                        questionId);

            var data =
                from answer in answers
                select new
                {
                    id = answer.Id,

                    questionId =
                        answer.QuestionId,

                    questionText =
                        answer.Question != null
                            ? answer.Question.QuestionText
                            : "",

                    answerText =
                        answer.AnswerText,

                    isCorrect =
                        answer.IsCorrect
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
            int? questionId)
        {
            await LoadQuestions();

            // CREATE

            if (id == null || id == 0)
            {
                var answer =
                    new Answer();

                if (questionId.HasValue &&
                    questionId.Value > 0)
                {
                    answer.QuestionId =
                        questionId.Value;
                }

                return View(answer);
            }

            // EDIT

            var existingAnswer =
                await _answerService
                    .GetAnswerByIdAsync(
                        id.Value);

            if (existingAnswer == null)
                return NotFound();

            return View(existingAnswer);
        }

        // ==========================================
        // UPSERT POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            Answer answer)
        {
            if (!ModelState.IsValid)
            {
                await LoadQuestions();

                return View(answer);
            }

            bool answerExists =
                await _answerService
                    .IsAnswerExistsAsync(
                        answer.QuestionId,
                        answer.AnswerText,
                        answer.Id);

            if (answerExists)
            {
                ModelState.AddModelError(
                    nameof(answer.AnswerText),
                    "Đáp án này đã tồn tại trong Question.");

                await LoadQuestions();

                return View(answer);
            }

            if (answer.Id == 0)
            {
                await _answerService
                    .CreateAnswerAsync(answer);

                TempData["success"] =
                    "Thêm Answer thành công.";
            }
            else
            {
                await _answerService
                    .UpdateAnswerAsync(answer);

                TempData["success"] =
                    "Cập nhật Answer thành công.";
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
            var answer =
                await _answerService
                    .GetAnswerByIdAsync(id);

            if (answer == null)
                return NotFound();

            return View(answer);
        }

        // ==========================================
        // DELETE
        // ==========================================

        [HttpDelete]
        public async Task<IActionResult> Delete(
            int id)
        {
            bool result =
                await _answerService
                    .DeleteAnswerAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy Answer."
                });
            }

            return Json(new
            {
                success = true,
                message =
                    "Xóa Answer thành công."
            });
        }

        // ==========================================
        // SET CORRECT
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> SetCorrect(
            int id)
        {
            var answer =
                await _answerService
                    .GetAnswerByIdAsync(id);

            if (answer == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy Answer."
                });
            }

            await _answerService
                .SetCorrectAnswerAsync(id);

            return Json(new
            {
                success = true,
                message =
                    "Đã đặt Answer này là đáp án đúng."
            });
        }

        // ==========================================
        // SET INCORRECT
        // ==========================================

        [HttpPost]
        public async Task<IActionResult> SetIncorrect(
            int id)
        {
            var answer =
                await _answerService
                    .GetAnswerByIdAsync(id);

            if (answer == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy Answer."
                });
            }

            await _answerService
                .SetIncorrectAnswerAsync(id);

            return Json(new
            {
                success = true,
                message =
                    "Đã bỏ trạng thái đáp án đúng."
            });
        }

        // ==========================================
        // LOAD QUESTIONS
        // ==========================================

        private async Task LoadQuestions()
        {
            var questions =
                await _questionService
                    .GetAllQuestionAsync();

            ViewBag.Questions =
                new SelectList(
                    questions,
                    "Id",
                    "QuestionText");
        }
    }
}