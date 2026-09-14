using AppLearningEnglish.Business.IService;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ListeningLessonController : Controller
    {
        private readonly IListeningLessonService
            _listeningService;

        private readonly ILessonService
            _lessonService;

        private readonly IWebHostEnvironment
            _environment;

        public ListeningLessonController(
            IListeningLessonService listeningService,
            ILessonService lessonService,
            IWebHostEnvironment environment)
        {
            _listeningService =
                listeningService;

            _lessonService =
                lessonService;

            _environment =
                environment;
        }

        // ==========================================
        // INDEX
        // ==========================================

        public async Task<IActionResult> Index(
            int? lessonId)
        {
            await LoadLessons();

            if (lessonId.HasValue &&
                lessonId.Value > 0)
            {
                ViewBag.SelectedLessonId =
                    lessonId.Value;
            }

            return View();
        }

        // ==========================================
        // GET ALL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int? lessonId)
        {
            var listeningLessons =
                await _listeningService
                    .SearchListeningLessonAsync(
                        search,
                        lessonId);

            var data =
                from listening in listeningLessons
                select new
                {
                    id = listening.Id,

                    lessonId =
                        listening.LessonId,

                    lessonTitle =
                        listening.Lesson != null
                            ? listening.Lesson.Title
                            : "",

                    audioUrl =
                        listening.AudioUrl,

                    transcript =
                        listening.Transcript
                };

            return Json(new
            {
                data = data
            });
        }

        // ==========================================
        // UPSERT GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Upsert(
            int? id,
            int? lessonId)
        {
            await LoadLessons();

            // CREATE

            if (id == null || id == 0)
            {
                var listeningLesson =
                    new ListeningLesson();

                if (lessonId.HasValue &&
                    lessonId.Value > 0)
                {
                    listeningLesson.LessonId =
                        lessonId.Value;
                }

                return View(listeningLesson);
            }

            // EDIT

            var existingListening =
                await _listeningService
                    .GetListeningLessonByIdAsync(
                        id.Value);

            if (existingListening == null)
                return NotFound();

            return View(existingListening);
        }

        // ==========================================
        // UPSERT POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            ListeningLesson listeningLesson)
        {
            // Kiểm tra Lesson đã có Listening chưa

            bool lessonExists =
                await _listeningService
                    .IsLessonExistsAsync(
                        listeningLesson.LessonId,
                        listeningLesson.Id);

            if (lessonExists)
            {
                ModelState.AddModelError(
                    nameof(
                        listeningLesson.LessonId),
                    "Lesson này đã có Listening Lesson.");
            }

            // ======================================
            // UPLOAD AUDIO
            // ======================================

            if (listeningLesson.AudioFile != null)
            {
                string[] allowedExtensions =
                {
                    ".mp3",
                    ".wav",
                    ".ogg",
                    ".m4a"
                };

                string extension =
                    Path.GetExtension(
                        listeningLesson.AudioFile
                            .FileName)
                    .ToLower();

                if (!allowedExtensions
                    .Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(
                            listeningLesson.AudioFile),
                        "Chỉ hỗ trợ MP3, WAV, OGG hoặc M4A.");
                }

                // Giới hạn 20MB

                if (listeningLesson.AudioFile.Length >
                    20 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(
                            listeningLesson.AudioFile),
                        "File audio không được vượt quá 20MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadLessons();

                return View(listeningLesson);
            }

            // ======================================
            // SAVE NEW AUDIO
            // ======================================

            string? oldAudioUrl = null;

            if (listeningLesson.Id > 0)
            {
                var existing =
                    await _listeningService
                        .GetListeningLessonByIdAsync(
                            listeningLesson.Id);

                if (existing != null)
                {
                    oldAudioUrl =
                        existing.AudioUrl;
                }
            }

            if (listeningLesson.AudioFile != null)
            {
                string uploadFolder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "audio",
                        "listening");

                if (!Directory.Exists(
                    uploadFolder))
                {
                    Directory.CreateDirectory(
                        uploadFolder);
                }

                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(
                        listeningLesson
                            .AudioFile
                            .FileName);

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                using (
                    FileStream stream =
                        new FileStream(
                            filePath,
                            FileMode.Create))
                {
                    await listeningLesson
                        .AudioFile
                        .CopyToAsync(stream);
                }

                listeningLesson.AudioUrl =
                    "/audio/listening/"
                    + fileName;

                // Xóa file cũ

                if (!string.IsNullOrWhiteSpace(
                    oldAudioUrl))
                {
                    DeleteAudioFile(
                        oldAudioUrl);
                }
            }

            // ======================================
            // CREATE
            // ======================================

            if (listeningLesson.Id == 0)
            {
                await _listeningService
                    .CreateListeningLessonAsync(
                        listeningLesson);

                TempData["success"] =
                    "Thêm Listening thành công.";
            }

            // ======================================
            // UPDATE
            // ======================================

            else
            {
                await _listeningService
                    .UpdateListeningLessonAsync(
                        listeningLesson);

                TempData["success"] =
                    "Cập nhật Listening thành công.";
            }

            return RedirectToAction(
                nameof(Index));
        }

        // ==========================================
        // DETAILS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var listening =
                await _listeningService
                    .GetListeningLessonByIdAsync(id);

            if (listening == null)
                return NotFound();

            return View(listening);
        }

        // ==========================================
        // DELETE
        // ==========================================

        [HttpDelete]
        public async Task<IActionResult> Delete(
            int id)
        {
            var listening =
                await _listeningService
                    .GetListeningLessonByIdAsync(id);

            if (listening == null)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy Listening."
                });
            }

            string? audioUrl =
                listening.AudioUrl;

            bool result =
                await _listeningService
                    .DeleteListeningLessonAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không thể xóa Listening."
                });
            }

            // Xóa file audio

            if (!string.IsNullOrWhiteSpace(
                audioUrl))
            {
                DeleteAudioFile(audioUrl);
            }

            return Json(new
            {
                success = true,
                message =
                    "Xóa Listening thành công."
            });
        }

        // ==========================================
        // LOAD LESSONS
        // ==========================================

        private async Task LoadLessons()
        {
            var lessons =
                await _lessonService
                    .GetAllLessonAsync();

            ViewBag.Lessons =
                new SelectList(
                    lessons,
                    "Id",
                    "Title");
        }

        // ==========================================
        // DELETE AUDIO FILE
        // ==========================================

        private void DeleteAudioFile(
            string audioUrl)
        {
            if (string.IsNullOrWhiteSpace(
                audioUrl))
            {
                return;
            }

            string relativePath =
                audioUrl.TrimStart('/')
                    .Replace(
                        "/",
                        Path.DirectorySeparatorChar
                            .ToString());

            string filePath =
                Path.Combine(
                    _environment.WebRootPath,
                    relativePath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}