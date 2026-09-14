using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TopicController : Controller
    {
        private readonly ITopicService _topicService;

        public TopicController(ITopicService topicService)
        {
            _topicService = topicService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var topics = await _topicService.GetAllAsync();

            return Json(new
            {
                data = topics
            });
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Topic());
            }

            var topic = await _topicService.GetTopicByIdAsync(id.Value);

            if (topic == null)
                return NotFound();

            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Topic topic)
        {
            if (!ModelState.IsValid)
                return View(topic);

            var exists = await _topicService
                .IsNameExistsAsync(topic.Name, topic.Id);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(topic.Name),
                    "Tên Topic đã tồn tại.");

                return View(topic);
            }

            if (topic.Id == 0)
            {
                await _topicService.CreateTopicAsync(topic);
                TempData["success"] = "Thêm Topic thành công";
            }
            else
            {
                await _topicService.UpdateTopicAsync(topic);
                TempData["success"] = "Cập nhật Topic thành công";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var topic = await _topicService.GetTopicByIdAsync(id);

            if (topic == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy Topic."
                });
            }

            await _topicService.DeleteTopicAsync(id);

            return Json(new
            {
                success = true,
                message = "Xóa Topic thành công."
            });
        }
    }
}