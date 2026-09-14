using AppLearningEnglish.Business;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IUserCourseService _userCourseService;
        private readonly ILessonService _lessonService;
        private readonly ILearningAccessService _learningAccess;

        public CourseController(
            ICourseService courseService,
            IUserCourseService userCourseService,
            ILessonService lessonService,
            ILearningAccessService learningAccess)
        {
            _courseService = courseService;
            _userCourseService = userCourseService;
            _lessonService = lessonService;
            _learningAccess = learningAccess;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var courses = (await _courseService.GetAllCourseAsync())
                .Where(x => x.IsPublished)
                .ToList();
            var userId = TryGetUserId();
            var userLevel = string.IsNullOrEmpty(userId)
                ? null
                : await _learningAccess.GetUserLevelAsync(userId);

            var cards = courses.Select(course => new CourseCatalogItemViewModel
            {
                Course = course,
                IsRecommended = EnglishLevelScale.IsRecommended(userLevel, course.Level),
                IsLocked = !string.IsNullOrEmpty(userId)
                    && EnglishLevelScale.IsTooHard(userLevel, course.Level)
            }).ToList();

            ViewBag.UserLevel = userLevel;
            return View(cards);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);

            if (course == null || !course.IsPublished)
            {
                return NotFound();
            }

            var lessons = await _lessonService.GetPublishedByCourseIdAsync(id);
            var userId = TryGetUserId();
            var enrolled = false;
            decimal progress = 0;
            var userLevel = string.IsNullOrEmpty(userId)
                ? null
                : await _learningAccess.GetUserLevelAsync(userId);
            var locked = !string.IsNullOrEmpty(userId)
                && EnglishLevelScale.IsTooHard(userLevel, course.Level);

            if (!string.IsNullOrEmpty(userId))
            {
                enrolled = await _userCourseService.IsEnrolledAsync(userId, id);

                if (enrolled)
                {
                    progress = await _userCourseService.GetProgressAsync(userId, id);
                }
            }

            return View(new CourseDetailsViewModel
            {
                Course = course,
                IsEnrolled = enrolled,
                Progress = progress,
                Lessons = lessons.ToList(),
                IsLocked = locked && !enrolled,
                IsRecommended = EnglishLevelScale.IsRecommended(userLevel, course.Level),
                UserLevel = userLevel
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            string userId = GetUserId();
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course != null && !await _learningAccess.CanAccessCourseLevelAsync(userId, course.Level))
            {
                TempData["error"] = $"Khóa {course.Level} đang khóa với trình độ của bạn. Hãy làm bài xếp lớp hoặc chọn khóa dễ hơn.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            bool result = await _userCourseService.EnrollAsync(userId, courseId);

            if (!result)
            {
                TempData["error"] = "Không thể đăng ký khóa học.";
            }
            else
            {
                TempData["success"] = "Đăng ký khóa học thành công.";
            }

            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        [Authorize]
        public async Task<IActionResult> MyCourses()
        {
            string userId = GetUserId();
            var courses = await _userCourseService.GetMyCoursesAsync(userId);
            return View(courses);
        }

        [Authorize]
        public async Task<IActionResult> InProgress()
        {
            string userId = GetUserId();
            var courses = await _userCourseService.GetInProgressCoursesAsync(userId);
            return View(courses);
        }

        [Authorize]
        public async Task<IActionResult> Completed()
        {
            string userId = GetUserId();
            var courses = await _userCourseService.GetCompletedCoursesAsync(userId);
            return View(courses);
        }

        [Authorize]
        public async Task<IActionResult> Continue(int courseId)
        {
            string userId = GetUserId();

            bool enrolled = await _userCourseService.IsEnrolledAsync(userId, courseId);

            if (!enrolled)
            {
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            await _userCourseService.UpdateProgressAsync(userId, courseId);

            var lesson = await _userCourseService.GetContinueLessonAsync(userId, courseId);

            if (lesson == null)
            {
                return RedirectToAction(nameof(Completed));
            }

            return RedirectToAction("Details", "Lesson", new { area = "Customer", id = lesson.Id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(int courseId)
        {
            string userId = GetUserId();
            await _userCourseService.UpdateProgressAsync(userId, courseId);
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEnrollment(int courseId)
        {
            string userId = GetUserId();
            await _userCourseService.RemoveEnrollmentAsync(userId, courseId);
            TempData["success"] = "Đã hủy đăng ký khóa học.";
            return RedirectToAction(nameof(MyCourses));
        }

        private string? TryGetUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        private string GetUserId()
        {
            var userId = TryGetUserId();

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException();
            }

            return userId;
        }
    }
}
