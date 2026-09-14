using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using AppLearningEnglish.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CertificateController : Controller
    {
        private readonly IUserCourseService _userCourseService;
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CertificateController(
            IUserCourseService userCourseService,
            ICourseService courseService,
            UserManager<ApplicationUser> userManager)
        {
            _userCourseService = userCourseService;
            _courseService = courseService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Course(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var enrollment = await _userCourseService.GetAsync(user.Id, id);
            var progress = enrollment == null
                ? 0
                : await _userCourseService.GetProgressAsync(user.Id, id);

            if (enrollment == null || progress < 100)
            {
                TempData["error"] = "Hoàn thành khóa học để nhận chứng chỉ.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id });
            }

            // Đảm bảo đã có mã chứng chỉ trước khi hiển thị.
            await _userCourseService.CompleteCourseAsync(user.Id, id);
            enrollment = await _userCourseService.GetAsync(user.Id, id);

            ViewBag.UserName = user.FullName ?? user.Email;
            ViewBag.CompletedAt = enrollment?.CompletedAt ?? DateTime.UtcNow;
            ViewBag.CertificateCode = enrollment?.CertificateCode;
            return View(course);
        }

        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var progress = await _userCourseService.GetProgressAsync(user.Id, id);
            if (progress < 100)
            {
                TempData["error"] = "Hoàn thành khóa học để tải chứng chỉ.";
                return RedirectToAction("Details", "Course", new { area = "Customer", id });
            }

            await _userCourseService.CompleteCourseAsync(user.Id, id);
            var enrollment = await _userCourseService.GetAsync(user.Id, id);

            var pdf = CertificatePdfBuilder.Build(
                user.FullName ?? user.Email ?? "Học viên",
                course.Name,
                course.Level,
                enrollment?.CompletedAt ?? DateTime.UtcNow,
                enrollment?.CertificateCode ?? "—");

            return File(
                pdf,
                "application/pdf",
                $"certificate-{enrollment?.CertificateCode ?? course.Id.ToString()}.pdf");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Verify(string? code)
        {
            ViewBag.Code = code;

            if (!string.IsNullOrWhiteSpace(code))
            {
                ViewBag.Enrollment = await _userCourseService.GetByCertificateCodeAsync(code);
            }

            return View();
        }
    }
}
