using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PlacementItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlacementItemController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.PlacementItems
                .AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return View(new PlacementItem { SortOrder = 1 });
            }

            var item = await _context.PlacementItems.FindAsync(id.Value);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(PlacementItem model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                _context.PlacementItems.Add(model);
                TempData["success"] = "Đã thêm câu xếp lớp.";
            }
            else
            {
                _context.PlacementItems.Update(model);
                TempData["success"] = "Đã cập nhật câu xếp lớp.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.PlacementItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy câu." });
            }

            _context.PlacementItems.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa câu." });
        }
    }
}
