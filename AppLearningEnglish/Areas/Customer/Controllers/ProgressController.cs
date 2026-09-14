using System.Text;
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
    public class ProgressController : Controller
    {
        private readonly IUserLessonService _userLessonService;
        private readonly ILearnerWorkspaceService _workspace;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProgressController(
            IUserLessonService userLessonService,
            ILearnerWorkspaceService workspace,
            UserManager<ApplicationUser> userManager)
        {
            _userLessonService = userLessonService;
            _workspace = workspace;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var userLessons =
                await _userLessonService.GetUserLessonsAsync(userId);

            return View(userLessons);
        }

        public async Task<IActionResult> Completed()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var lessons =
                await _userLessonService
                    .GetCompletedLessonsAsync(userId);

            return View(lessons);
        }

        public async Task<IActionResult> InProgress()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var lessons =
                await _userLessonService
                    .GetInProgressLessonsAsync(userId);

            return View(lessons);
        }

        public async Task<IActionResult> Skills()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            return View(await _workspace.GetSkillsAsync(userId));
        }

        public async Task<IActionResult> Charts()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            return View(await _workspace.GetSkillsAsync(userId));
        }

        public async Task<IActionResult> ExportCsv()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var rows = await _workspace.GetExportRowsAsync(userId);
            var builder = new StringBuilder();
            builder.AppendLine("Khoa,Bai,Tien do,Trang thai,Tu,Nghe,Bai tap");

            foreach (var row in rows)
            {
                builder.AppendLine(string.Join(',',
                    Csv(row.Course),
                    Csv(row.Lesson),
                    row.Progress.ToString("0"),
                    Csv(row.Status),
                    Csv(row.Vocab),
                    Csv(row.Listening),
                    Csv(row.Exercise)));
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
            return File(bytes, "text/csv", "tien-do.csv");
        }

        public async Task<IActionResult> ExportPdf()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var rows = await _workspace.GetExportRowsAsync(user.Id);
            var name = string.IsNullOrWhiteSpace(user.FullName) ? user.Email ?? "Hoc vien" : user.FullName;
            var pdf = ProgressPdfBuilder.Build(name, rows);
            return File(pdf, "application/pdf", "tien-do.pdf");
        }

        private static string Csv(string? value)
        {
            var text = (value ?? string.Empty).Replace("\"", "\"\"");
            return $"\"{text}\"";
        }
    }
}