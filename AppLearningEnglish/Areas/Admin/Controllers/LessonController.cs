using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppLearningEnglish.Models;
using AppLearningEnglish.Business.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LessonController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseService _courseService;
        private readonly ITopicService _topicService;

        public LessonController(
            ILessonService lessonService,
            ICourseService courseService,
            ITopicService topicService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
            _topicService = topicService;
        }

        // =========================
        // INDEX
        // =========================

        public async Task<IActionResult> Index()
        {
            await LoadDropdowns();

            return View();
        }

        // =========================
        // GET ALL
        // =========================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int? courseId,
            int? topicId,
            bool? isPublished)
        {
            var lessons =
                await _lessonService.SearchLessonAsync(
                    search,
                    courseId,
                    topicId,
                    isPublished);

            return Json(new
            {
                data = lessons
            });
        }

        // =========================
        // UPSERT GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            await LoadDropdowns();

            if (id == null || id == 0)
            {
                var lesson = new Lesson();

                return View(lesson);
            }

            var existingLesson =
                await _lessonService
                    .GetLessonByIdAsync(id.Value);

            if (existingLesson == null)
                return NotFound();

            return View(existingLesson);
        }

        // =========================
        // UPSERT POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            Lesson lesson)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();

                return View(lesson);
            }

            bool titleExists =
                await _lessonService.IsTitleExistsAsync(
                    lesson.Title,
                    lesson.CourseId,
                    lesson.Id);

            if (titleExists)
            {
                ModelState.AddModelError(
                    nameof(lesson.Title),
                    "Tên Lesson đã tồn tại trong Course này.");

                await LoadDropdowns();

                return View(lesson);
            }

            if (lesson.Id == 0)
            {
                await _lessonService
                    .CreateLessonAsync(lesson);

                TempData["success"] =
                    "Thêm Lesson thành công.";
            }
            else
            {
                await _lessonService
                    .UpdateLessonAsync(lesson);

                TempData["success"] =
                    "Cập nhật Lesson thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS
        // =========================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var lesson =
                await _lessonService
                    .GetLessonByIdAsync(id);

            if (lesson == null)
                return NotFound();

            return View(lesson);
        }

        // =========================
        // DELETE
        // =========================

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var lesson =
                await _lessonService
                    .GetLessonByIdAsync(id);

            if (lesson == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy Lesson."
                });
            }

            bool result =
                await _lessonService
                    .DeleteLessonAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message = "Xóa Lesson thất bại."
                });
            }

            return Json(new
            {
                success = true,
                message = "Xóa Lesson thành công."
            });
        }

        // =========================
        // PUBLISH
        // =========================

        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            await _lessonService
                .PublishLessonAsync(id);

            return Json(new
            {
                success = true,
                message = "Publish Lesson thành công."
            });
        }

        // =========================
        // UNPUBLISH
        // =========================

        [HttpPost]
        public async Task<IActionResult> Unpublish(int id)
        {
            await _lessonService
                .UnpublishLessonAsync(id);

            return Json(new
            {
                success = true,
                message = "Unpublish Lesson thành công."
            });
        }

        // =========================
        // DROPDOWN
        // =========================

        private async Task LoadDropdowns()
        {
            var courses =
                await _courseService
                    .GetAllCourseAsync();

            var topics =
                await _topicService
                    .GetAllAsync();

            ViewBag.Courses =
                new SelectList(
                    courses,
                    "Id",
                    "Name");

            ViewBag.Topics =
                new SelectList(
                    topics,
                    "Id",
                    "Name");
        }
    }
}