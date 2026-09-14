using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class PointController : Controller
    {
        private readonly IUserPointService _userPointService;
        private readonly ApplicationDbContext _context;

        public PointController(
            IUserPointService userPointService,
            ApplicationDbContext context)
        {
            _userPointService = userPointService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string userId = GetUserId();
            var userPoint = await _userPointService.GetAsync(userId)
                ?? await _userPointService.CreateAsync(userId);
            return View(userPoint);
        }

        public async Task<IActionResult> Leaderboard(string period = "all")
        {
            ViewBag.Period = period;
            if (period == "all")
            {
                return View(await _userPointService.GetLeaderboardAsync(20));
            }

            var from = period == "month"
                ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))
                : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));

            var rows = await _context.DailyStreaks
                .AsNoTracking()
                .Where(x => x.StudyDate >= from)
                .GroupBy(x => x.ApplicationUserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Points = g.Sum(x => x.PointsEarned)
                })
                .OrderByDescending(x => x.Points)
                .Take(20)
                .ToListAsync();

            var ids = rows.Select(r => r.UserId).ToList();
            var users = await _context.applicationUsers
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            var board = rows.Select(r => new AppLearningEnglish.Models.UserPoint
            {
                UserId = r.UserId,
                Points = r.Points,
                ApplicationUser = users.FirstOrDefault(u => u.Id == r.UserId)
            }).ToList();

            return View(board);
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
