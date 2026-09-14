using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using AppLearningEnglish.Models;
using AppLearningEnglish.Business.Services.IServices;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AchievementController : Controller
    {
        private readonly IAchievementService
            _achievementService;

        public AchievementController(
            IAchievementService achievementService)
        {
            _achievementService =
                achievementService;
        }

        // =========================================
        // INDEX
        // =========================================

        public async Task<IActionResult> Index()
        {
            var achievements =
                await _achievementService
                    .GetAllAsync();

            return View(achievements);
        }

        // =========================================
        // UPSERT GET
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Upsert(
            int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return View(
                    new Achievement());
            }

            var achievement =
                await _achievementService
                    .GetByIdAsync(id.Value);

            if (achievement == null)
            {
                return NotFound();
            }

            return View(achievement);
        }

        // =========================================
        // UPSERT POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            Achievement achievement)
        {
            if (!ModelState.IsValid)
            {
                return View(achievement);
            }

            bool nameExists =
                await _achievementService
                    .IsNameExistsAsync(
                        achievement.Name,
                        achievement.Id);

            if (nameExists)
            {
                ModelState.AddModelError(
                    nameof(achievement.Name),
                    "Achievement đã tồn tại.");

                return View(achievement);
            }

            if (achievement.Id == 0)
            {
                await _achievementService
                    .CreateAsync(achievement);

                TempData["success"] =
                    "Tạo Achievement thành công.";
            }
            else
            {
                await _achievementService
                    .UpdateAsync(achievement);

                TempData["success"] =
                    "Cập nhật Achievement thành công.";
            }

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================
        // DELETE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            bool result =
                await _achievementService
                    .DeleteAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Achievement không tồn tại."
                });
            }

            return Json(new
            {
                success = true,
                message =
                    "Xóa Achievement thành công."
            });
        }
    }
}