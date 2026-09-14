using AppLearningEnglish.Business.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppLearningEnglish.Models;

namespace AppLearningEnglish.ViewComponents
{
    public class StudyReminderViewComponent : ViewComponent
    {
        private readonly IStudyReminderService _studyReminderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudyReminderViewComponent(
            IStudyReminderService studyReminderService,
            UserManager<ApplicationUser> userManager)
        {
            _studyReminderService = studyReminderService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User);

            if (string.IsNullOrEmpty(userId))
            {
                return Content(string.Empty);
            }

            var inbox = await _studyReminderService.GetInboxAsync(userId);
            return View(inbox);
        }
    }
}
