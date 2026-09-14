using AppLearningEnglish.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PracticeAttemptController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PracticeAttemptController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? kind)
        {
            var query = _context.PracticeAttempts
                .AsNoTracking()
                .Include(x => x.ApplicationUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(kind))
            {
                query = query.Where(x => x.Kind == kind);
            }

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(200)
                .ToListAsync();

            ViewData["Kind"] = kind;
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.PracticeAttempts
                .AsNoTracking()
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == id);
            return item == null ? NotFound() : View(item);
        }
    }
}
