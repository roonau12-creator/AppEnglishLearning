using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ReminderController : Controller
    {
        private readonly IStudyReminderService _reminders;

        public ReminderController(IStudyReminderService reminders)
        {
            _reminders = reminders;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            await _reminders.EnsureTodayLogsAsync(userId);
            var logs = await _reminders.GetLogsAsync(userId);
            var settings = await _reminders.GetSettingsAsync(userId);

            return View(new ReminderPageViewModel
            {
                Settings = settings,
                Logs = logs
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings([Bind(Prefix = "Settings")] ReminderSettingsViewModel settings)
        {
            await _reminders.SaveSettingsAsync(GetUserId(), settings);
            TempData["success"] = settings.ReminderEnabled
                ? $"Đã lưu nhắc học lúc {settings.ReminderHour:00}:00 ({settings.TimeZoneId})."
                : "Đã tắt nhắc học.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead()
        {
            await _reminders.MarkLogsReadAsync(GetUserId());
            TempData["success"] = "Đã đánh dấu tất cả nhắc là đã đọc.";
            return RedirectToAction(nameof(Index));
        }

        private string GetUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
            {
                throw new UnauthorizedAccessException();
            }

            return claim.Value;
        }
    }
}
