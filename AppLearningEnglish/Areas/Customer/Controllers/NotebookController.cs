using AppLearningEnglish.Business.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class NotebookController : Controller
    {
        private readonly ILearnerWorkspaceService _workspace;

        public NotebookController(ILearnerWorkspaceService workspace)
        {
            _workspace = workspace;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            ViewBag.Bookmarks = await _workspace.GetBookmarksAsync(userId);
            var notes = await _workspace.GetNotesAsync(userId);
            return View(notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(int id)
        {
            await _workspace.DeleteNoteAsync(GetUserId(), id);
            TempData["success"] = "Đã xóa ghi chú.";
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
