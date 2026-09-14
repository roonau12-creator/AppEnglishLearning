using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppLearningEnglish.Models;
using AppLearningEnglish.Business.Services.IServices;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
     [Area("Admin")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;

        public CourseController(
            ICourseService courseService)
        {
            _courseService = courseService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            string? level,
            bool? isPublished)
        {
            var courses =
                await _courseService.SearchCourseAsync(
                    search,
                    level,
                    isPublished);

            return Json(new
            {
                data = courses
            });
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Course());
            }

            var course =
                await _courseService
                    .GetCourseByIdAsync(id.Value);

            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            Course course)
        {
            if (!ModelState.IsValid)
            {
                return View(course);
            }

            bool nameExists =
                await _courseService.IsNameExistsAsync(
                    course.Name,
                    course.Id);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(course.Name),
                    "Tên Course đã tồn tại.");

                return View(course);
            }

            if (course.Id == 0)
            {
                await _courseService
                    .CreateCourseAsync(course);

                TempData["success"] =
                    "Thêm Course thành công.";
            }
            else
            {
                await _courseService
                    .UpdateCourseAsync(course);

                TempData["success"] =
                    "Cập nhật Course thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var course =
                await _courseService
                    .GetCourseByIdAsync(id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var course =
                await _courseService
                    .GetCourseByIdAsync(id);

            if (course == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy Course."
                });
            }

            bool result =
                await _courseService
                    .DeleteCourseAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không thể xóa Course vì Course đang có Lesson."
                });
            }

            return Json(new
            {
                success = true,
                message = "Xóa Course thành công."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            await _courseService
                .PublishCourseAsync(id);

            return Json(new
            {
                success = true,
                message = "Publish Course thành công."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Unpublish(int id)
        {
            await _courseService
                .UnpublishCourseAsync(id);

            return Json(new
            {
                success = true,
                message = "Unpublish Course thành công."
            });
        }
    }
}