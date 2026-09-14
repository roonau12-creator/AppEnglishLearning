using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private const int TrendDays = 14;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var enrollments = await _context.UserCourses
                .Include("Course")
                .ToListAsync();

            var completedAttempts = await _context.ExerciseAttempts
                .Where(x => x.CompletedAt != null)
                .Select(x => new { x.Score, x.CompletedAt })
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);

            var model = new AdminDashboardViewModel
            {
                Users = await _userManager.Users.CountAsync(),
                Courses = await _context.courses.CountAsync(),
                Lessons = await _context.Lessons.CountAsync(),
                Enrollments = enrollments.Count,
                Attempts = await _context.ExerciseAttempts.CountAsync(),
                Words = await _context.words.CountAsync(),
                CompletedEnrollments = enrollments.Count(x => x.Progress >= 100),
                ActiveLearnersToday = await _context.DailyStreaks
                    .Where(x => x.StudyDate == today)
                    .Select(x => x.ApplicationUserId)
                    .Distinct()
                    .CountAsync()
            };

            if (enrollments.Count > 0)
            {
                model.CompletionRate = Math.Round(
                    (decimal)model.CompletedEnrollments / enrollments.Count * 100,
                    1);
            }

            if (completedAttempts.Count > 0)
            {
                model.AverageScore = Math.Round(
                    completedAttempts.Average(x => x.Score),
                    1);
            }

            var firstDay = DateTime.Now.Date.AddDays(-(TrendDays - 1));

            for (var day = firstDay; day <= DateTime.Now.Date; day = day.AddDays(1))
            {
                var label = day.ToString("dd/MM");

                model.EnrollmentTrend.Add(new ChartPointViewModel
                {
                    Label = label,
                    Value = enrollments.Count(x => x.EnrolledAt.Date == day)
                });

                model.AttemptTrend.Add(new ChartPointViewModel
                {
                    Label = label,
                    Value = completedAttempts.Count(x => x.CompletedAt!.Value.Date == day)
                });
            }

            model.TopCourses = enrollments
                .GroupBy(x => x.Course?.Name ?? "(không rõ)")
                .Select(group => new CourseStatViewModel
                {
                    CourseName = group.Key,
                    Enrollments = group.Count(),
                    AverageProgress = Math.Round(group.Average(x => x.Progress), 1)
                })
                .OrderByDescending(x => x.Enrollments)
                .Take(5)
                .ToList();

            return View(model);
        }
    }
}
