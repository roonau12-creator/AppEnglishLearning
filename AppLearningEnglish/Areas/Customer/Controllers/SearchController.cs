using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class SearchController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;
        private readonly IWordService _wordService;
        private readonly ApplicationDbContext _context;

        public SearchController(
            ICourseService courseService,
            ILessonService lessonService,
            IWordService wordService,
            ApplicationDbContext context)
        {
            _courseService = courseService;
            _lessonService = lessonService;
            _wordService = wordService;
            _context = context;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var model = new SearchResultViewModel { Query = q };

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(model);
            }

            var term = q.Trim();
            var like = term.ToLower();
            var courses = await _courseService.SearchCourseAsync(term, null, true);
            var lessons = await _lessonService.SearchLessonAsync(term, null, null, true);
            var words = await _wordService.SearchWordAsync(term, null);

            model.Courses = courses.ToList();
            model.Lessons = lessons.Where(x => x.IsPublished).ToList();
            model.Words = words.ToList();
            model.Passages = await _context.ReadingPassages.AsNoTracking()
                .Where(x =>
                    x.Title.ToLower().Contains(like)
                    || x.Body.ToLower().Contains(like)
                    || (x.Topic != null && x.Topic.ToLower().Contains(like)))
                .Take(20)
                .ToListAsync();
            model.GrammarTopics = await _context.GrammarTopics.AsNoTracking()
                .Where(x =>
                    x.Title.ToLower().Contains(like)
                    || x.Explanation.ToLower().Contains(like)
                    || (x.Examples != null && x.Examples.ToLower().Contains(like)))
                .Take(20)
                .ToListAsync();
            model.WritingPrompts = await _context.WritingPrompts.AsNoTracking()
                .Where(x =>
                    x.Title.ToLower().Contains(like)
                    || x.Prompt.ToLower().Contains(like)
                    || (x.KeyPoints != null && x.KeyPoints.ToLower().Contains(like)))
                .Take(20)
                .ToListAsync();

            return View(model);
        }
    }
}
