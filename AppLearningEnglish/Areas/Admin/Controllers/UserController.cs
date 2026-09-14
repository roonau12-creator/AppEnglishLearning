using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IUserCourseService _userCourseService;
        private readonly IDailyStreakService _dailyStreakService;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IUserCourseService userCourseService,
            IDailyStreakService dailyStreakService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _userCourseService = userCourseService;
            _dailyStreakService = dailyStreakService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var data = new List<object>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                data.Add(new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    englishLevel = user.EnglishLevel,
                    locked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    roles = string.Join(", ", roles)
                });
            }

            return Json(new { data });
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id ?? string.Empty);

            if (user == null)
            {
                return NotFound();
            }

            var enrollments = (await _userCourseService.GetMyCoursesAsync(user.Id)).ToList();
            var enrolledCourseIds = enrollments.Select(x => x.CourseId).ToHashSet();

            var model = new AdminUserDetailViewModel
            {
                User = user,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                Points = await _context.UserPoints
                    .Where(x => x.UserId == user.Id)
                    .Select(x => x.Points)
                    .FirstOrDefaultAsync(),
                CurrentStreak = await _dailyStreakService.GetCurrentStreakAsync(user.Id),
                TotalStudyMinutes = await _dailyStreakService.GetTotalStudyMinutesAsync(user.Id),
                Enrollments = enrollments,
                Lessons = await _context.UserLessons
                    .Include("Lesson")
                    .Where(x => x.ApplicationUserId == user.Id)
                    .OrderByDescending(x => x.Progress)
                    .ToListAsync(),
                Attempts = await _context.ExerciseAttempts
                    .Include("Exercise")
                    .Where(x => x.ApplicationUserId == user.Id)
                    .OrderByDescending(x => x.StartedAt)
                    .Take(20)
                    .ToListAsync(),
                PracticeAttempts = await _context.PracticeAttempts
                    .AsNoTracking()
                    .Where(x => x.ApplicationUserId == user.Id)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(20)
                    .ToListAsync(),
                AvailableCourses = await _context.courses
                    .Where(x => !enrolledCourseIds.Contains(x.Id))
                    .OrderBy(x => x.Name)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(string id, int courseId)
        {
            var user = await _userManager.FindByIdAsync(id ?? string.Empty);

            if (user == null)
            {
                TempData["error"] = "Không tìm thấy user.";
                return RedirectToAction(nameof(Index));
            }

            await _userCourseService.EnrollAsync(user.Id, courseId);
            TempData["success"] = "Đã đăng ký khóa học cho học viên.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(string id, int courseId)
        {
            var user = await _userManager.FindByIdAsync(id ?? string.Empty);

            if (user == null)
            {
                TempData["error"] = "Không tìm thấy user.";
                return RedirectToAction(nameof(Index));
            }

            await _userCourseService.RemoveEnrollmentAsync(user.Id, courseId);
            TempData["success"] = "Đã hủy đăng ký và xóa tiến độ khóa học đó.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy user." });
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            return Json(new { success = true, message = "Đã khóa tài khoản." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy user." });
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            return Json(new { success = true, message = "Đã mở khóa tài khoản." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || (role != AppRoles.Admin && role != AppRoles.User))
            {
                return Json(new { success = false, message = "Không hợp lệ." });
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var current = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, current);
            await _userManager.AddToRoleAsync(user, role);
            return Json(new { success = true, message = "Đã cập nhật vai trò." });
        }
    }
}
