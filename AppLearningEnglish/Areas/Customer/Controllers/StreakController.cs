using System.Security.Claims;
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
    public class StreakController : Controller
    {
        private readonly IDailyStreakService
            _dailyStreakService;

        private readonly UserManager<ApplicationUser> _userManager;

        public StreakController(
            IDailyStreakService dailyStreakService,
            UserManager<ApplicationUser> userManager)
        {
            _dailyStreakService =
                dailyStreakService;

            _userManager = userManager;
        }


        // =========================================
        // INDEX
        // =========================================

        public async Task<IActionResult> Index(
            int? year,
            int? month)
        {
            string userId = GetUserId();

            DateTime now =
                DateTime.Now;

            int selectedYear =
                year ?? now.Year;

            int selectedMonth =
                month ?? now.Month;

            if (selectedMonth < 1)
            {
                selectedMonth = 12;
                selectedYear--;
            }

            if (selectedMonth > 12)
            {
                selectedMonth = 1;
                selectedYear++;
            }


            // =====================================
            // TODAY
            // =====================================

            var today =
                await _dailyStreakService
                    .GetTodayAsync(userId);


            // =====================================
            // STATISTICS
            // =====================================

            int currentStreak =
                await _dailyStreakService
                    .GetCurrentStreakAsync(userId);

            int longestStreak =
                await _dailyStreakService
                    .GetLongestStreakAsync(userId);

            int totalStudyDays =
                await _dailyStreakService
                    .GetTotalStudyDaysAsync(userId);

            int totalMinutes =
                await _dailyStreakService
                    .GetTotalStudyMinutesAsync(userId);

            int totalPoints =
                await _dailyStreakService
                    .GetTotalPointsAsync(userId);


            // =====================================
            // MONTH
            // =====================================

            var monthRecords =
                await _dailyStreakService
                    .GetMonthAsync(
                        userId,
                        selectedYear,
                        selectedMonth);


            // =====================================
            // CALENDAR
            // =====================================

            var model =
                new DailyStreakVM();

            model.Today =
                DateOnly.FromDateTime(now);

            model.TodayMinutes =
                today?.MinutesStudied ?? 0;

            model.TodayPoints =
                today?.PointsEarned ?? 0;

            model.StudiedToday =
                today != null &&
                today.MinutesStudied > 0;

            model.CurrentStreak =
                currentStreak;

            model.LongestStreak =
                longestStreak;

            model.TotalStudyDays =
                totalStudyDays;

            model.TotalStudyMinutes =
                totalMinutes;

            model.TotalPoints =
                totalPoints;


            // Daily goal
            var user = await _userManager.GetUserAsync(User);

            model.DailyGoalMinutes = user?.DailyGoalMinutes ?? 30;

            if (model.DailyGoalMinutes > 0)
            {
                model.DailyGoalProgress =
                    (int)Math.Min(
                        100,
                        Math.Round(
                            (decimal)model.TodayMinutes
                            / model.DailyGoalMinutes
                            * 100));
            }


            model.CalendarYear =
                selectedYear;

            model.CalendarMonth =
                selectedMonth;


            foreach (var item in monthRecords)
            {
                model.CalendarDays.Add(
                    new CalendarDayVM
                    {
                        Date =
                            item.StudyDate,

                        HasStudied =
                            item.MinutesStudied > 0,

                        MinutesStudied =
                            item.MinutesStudied,

                        PointsEarned =
                            item.PointsEarned
                    });
            }


            // =====================================
            // HISTORY
            // =====================================

            var history =
                await _dailyStreakService
                    .GetHistoryAsync(userId);

            foreach (var item in history)
            {
                model.History.Add(
                    new DailyStreakHistoryVM
                    {
                        StudyDate =
                            item.StudyDate,

                        MinutesStudied =
                            item.MinutesStudied,

                        PointsEarned =
                            item.PointsEarned
                    });
            }


            return View(model);
        }


        // =========================================
        // RECORD STUDY
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordStudy(
            int minutes,
            int points)
        {
            if (minutes < 0)
            {
                minutes = 0;
            }

            if (points < 0)
            {
                points = 0;
            }

            string userId = GetUserId();

            await _dailyStreakService
                .RecordStudyAsync(
                    userId,
                    minutes,
                    points);

            TempData["success"] =
                "Đã cập nhật thời gian học.";

            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // HISTORY
        // =========================================

        public async Task<IActionResult> History()
        {
            string userId = GetUserId();

            var records =
                await _dailyStreakService
                    .GetHistoryAsync(userId);

            var model =
                new DailyStreakVM();

            foreach (var item in records)
            {
                model.History.Add(
                    new DailyStreakHistoryVM
                    {
                        StudyDate =
                            item.StudyDate,

                        MinutesStudied =
                            item.MinutesStudied,

                        PointsEarned =
                            item.PointsEarned
                    });
            }

            model.CurrentStreak =
                await _dailyStreakService
                    .GetCurrentStreakAsync(userId);

            model.LongestStreak =
                await _dailyStreakService
                    .GetLongestStreakAsync(userId);

            model.TotalStudyDays =
                await _dailyStreakService
                    .GetTotalStudyDaysAsync(userId);

            model.TotalStudyMinutes =
                await _dailyStreakService
                    .GetTotalStudyMinutesAsync(userId);

            return View(model);
        }


        // =========================================
        // USER ID
        // =========================================

        private string GetUserId()
        {
            var claim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier);

            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
            {
                throw new UnauthorizedAccessException();
            }

            return claim.Value;
        }
    }
}