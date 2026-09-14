using AppLearningEnglish.Business.IService;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Service
{
    public class ListeningLessonService
        : IListeningLessonService
    {
        private readonly ApplicationDbContext _context;

        public ListeningLessonService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL
        // ==========================================

        public async Task<IEnumerable<ListeningLesson>>
            GetAllListeningLessonAsync()
        {
            var query =
                from listening in _context.ListeningLessons
                orderby listening.LessonId
                select listening;

            return await query
                .Include("Lesson.Course")
                .ToListAsync();
        }

        // ==========================================
        // SEARCH + FILTER
        // ==========================================

        public async Task<IEnumerable<ListeningLesson>>
            SearchListeningLessonAsync(
                string? search,
                int? lessonId)
        {
            IQueryable<ListeningLesson> query =
                _context.ListeningLessons;

            // SEARCH

            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query =
                    from listening in query
                    where
                        (
                            listening.AudioUrl != null
                            &&
                            listening.AudioUrl
                                .ToLower()
                                .Contains(keyword)
                        )
                        ||
                        (
                            listening.Transcript != null
                            &&
                            listening.Transcript
                                .ToLower()
                                .Contains(keyword)
                        )
                        ||
                        (
                            listening.Lesson != null
                            &&
                            listening.Lesson.Title
                                .ToLower()
                                .Contains(keyword)
                        )
                    select listening;
            }

            // FILTER LESSON

            if (lessonId.HasValue &&
                lessonId.Value > 0)
            {
                int selectedLessonId =
                    lessonId.Value;

                query =
                    from listening in query
                    where listening.LessonId ==
                          selectedLessonId
                    select listening;
            }

            query =
                from listening in query
                orderby listening.LessonId
                select listening;

            return await query
                .Include("Lesson.Course")
                .ToListAsync();
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        public async Task<ListeningLesson?>
            GetListeningLessonByIdAsync(
                int id)
        {
            var query =
                from listening in _context.ListeningLessons
                where listening.Id == id
                select listening;

            return await query
                .Include("Lesson")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // GET BY LESSON
        // ==========================================

        public async Task<ListeningLesson?>
            GetByLessonIdAsync(
                int lessonId)
        {
            var query =
                from listening in _context.ListeningLessons
                where listening.LessonId == lessonId
                select listening;

            return await query
                .Include("Lesson")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // CREATE
        // ==========================================

        public async Task CreateListeningLessonAsync(
            ListeningLesson listeningLesson)
        {
            if (!string.IsNullOrWhiteSpace(
                listeningLesson.AudioUrl))
            {
                listeningLesson.AudioUrl =
                    listeningLesson.AudioUrl.Trim();
            }

            if (!string.IsNullOrWhiteSpace(
                listeningLesson.Transcript))
            {
                listeningLesson.Transcript =
                    listeningLesson.Transcript.Trim();
            }

            _context.ListeningLessons.Add(
                listeningLesson);

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // UPDATE
        // ==========================================

        public async Task UpdateListeningLessonAsync(
            ListeningLesson listeningLesson)
        {
            var existingListening =
                await GetListeningLessonByIdAsync(
                    listeningLesson.Id);

            if (existingListening == null)
                return;

            existingListening.LessonId =
                listeningLesson.LessonId;

            existingListening.AudioUrl =
                listeningLesson.AudioUrl;

            existingListening.Transcript =
                listeningLesson.Transcript;

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // DELETE
        // ==========================================

        public async Task<bool>
            DeleteListeningLessonAsync(
                int id)
        {
            var listening =
                await GetListeningLessonByIdAsync(id);

            if (listening == null)
                return false;

            _context.ListeningLessons.Remove(
                listening);

            await _context.SaveChangesAsync();

            return true;
        }

        // ==========================================
        // CHECK LESSON
        // ==========================================

        public async Task<bool>
            IsLessonExistsAsync(
                int lessonId,
                int? id = null)
        {
            var query =
                from listening in _context.ListeningLessons
                where listening.LessonId == lessonId
                select listening;

            if (id.HasValue)
            {
                query =
                    from listening in query
                    where listening.Id != id.Value
                    select listening;
            }

            return await query.AnyAsync();
        }
    }
}