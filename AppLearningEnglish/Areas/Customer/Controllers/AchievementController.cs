using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using AppLearningEnglish.Business.Services.IServices;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
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
        // MY ACHIEVEMENTS
        // =========================================

        public async Task<IActionResult> Index()
        {
            string userId = GetUserId();

            await _achievementService
                .SyncAchievementsAsync(userId);

            var achievements =
                await _achievementService
                    .GetAllAsync();

            var userAchievements =
                await _achievementService
                    .GetUserAchievementsAsync(
                        userId);

            int points =
                await _achievementService
                    .GetUserPointsAsync(
                        userId);

            int unlockedCount =
                await _achievementService
                    .GetUnlockedCountAsync(
                        userId);

            int totalCount =
                await _achievementService
                    .GetTotalAchievementCountAsync();

            ViewBag.Points = points;

            ViewBag.UnlockedCount =
                unlockedCount;

            ViewBag.TotalCount =
                totalCount;

            var unlockedIds =
                new HashSet<int>();

            foreach (var item
                in userAchievements)
            {
                unlockedIds.Add(
                    item.AchievementId);
            }

            ViewBag.UnlockedIds =
                unlockedIds;

            return View(achievements);
        }

        // =========================================
        // DETAILS
        // =========================================

        public async Task<IActionResult> Details(
            int id)
        {
            string userId = GetUserId();

            await _achievementService
                .SyncAchievementsAsync(userId);

            var achievement =
                await _achievementService
                    .GetByIdAsync(id);

            if (achievement == null)
            {
                return NotFound();
            }

            bool unlocked =
                await _achievementService
                    .HasAchievementAsync(
                        userId,
                        id);

            int points =
                await _achievementService
                    .GetUserPointsAsync(
                        userId);

            ViewBag.Unlocked = unlocked;

            ViewBag.Points = points;

            return View(achievement);
        }

        // =========================================
        // USER ID
        // =========================================

        private string GetUserId()
        {
            var claim =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                throw new UnauthorizedAccessException();
            }

            return claim.Value;
        }
    }
}