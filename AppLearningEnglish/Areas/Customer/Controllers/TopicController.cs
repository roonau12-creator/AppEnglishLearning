using AppLearningEnglish.Business.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class TopicController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly ILessonService _lessonService;

        public TopicController(
            ITopicService topicService,
            ILessonService lessonService)
        {
            _topicService = topicService;
            _lessonService = lessonService;
        }

        public async Task<IActionResult> Index()
        {
            var topics = await _topicService.GetAllAsync();
            return View(topics);
        }

        public async Task<IActionResult> Details(int id)
        {
            var topic = await _topicService.GetTopicByIdAsync(id);
            if (topic == null)
            {
                return NotFound();
            }

            var lessons = await _lessonService.SearchLessonAsync(null, null, id, true);
            ViewBag.Topic = topic;
            return View(lessons.Where(x => x.IsPublished).ToList());
        }
    }
}
