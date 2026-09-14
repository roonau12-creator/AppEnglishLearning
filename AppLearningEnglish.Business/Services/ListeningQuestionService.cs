using AppLearningEnglish.Business.IService;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Service
{
    public class ListeningQuestionService
        : IListeningQuestionService
    {
        private readonly ApplicationDbContext _context;

        public ListeningQuestionService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL
        // ==========================================

        public async Task<IEnumerable<ListeningQuestion>>
            GetAllListeningQuestionAsync()
        {
            var query =
                from item in _context.ListeningQuestions
                orderby item.ListeningLessonId, item.Id
                select item;

            return await query
                .Include("ListeningLesson")
                .ToListAsync();
        }

        // ==========================================
        // SEARCH + FILTER
        // ==========================================

        public async Task<IEnumerable<ListeningQuestion>>
            SearchListeningQuestionAsync(
                string? search,
                int? listeningLessonId)
        {
            IQueryable<ListeningQuestion> query =
                _context.ListeningQuestions;

            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query =
                    from item in query
                    where
                        item.Question
                            .ToLower()
                            .Contains(keyword)
                        ||
                        item.Answer
                            .ToLower()
                            .Contains(keyword)
                        ||
                        (
                            item.ListeningLesson != null
                            &&
                            item.ListeningLesson.Transcript != null
                            &&
                            item.ListeningLesson.Transcript
                                .ToLower()
                                .Contains(keyword)
                        )
                    select item;
            }

            if (listeningLessonId.HasValue &&
                listeningLessonId.Value > 0)
            {
                int selectedId =
                    listeningLessonId.Value;

                query =
                    from item in query
                    where item.ListeningLessonId == selectedId
                    select item;
            }

            query =
                from item in query
                orderby item.ListeningLessonId, item.Id
                select item;

            return await query
                .Include("ListeningLesson")
                .ToListAsync();
        }

        // ==========================================
        // GET BY LISTENING LESSON
        // ==========================================

        public async Task<IEnumerable<ListeningQuestion>>
            GetByListeningLessonIdAsync(
                int listeningLessonId)
        {
            var query =
                from item in _context.ListeningQuestions
                where item.ListeningLessonId ==
                      listeningLessonId
                orderby item.Id
                select item;

            return await query
                .Include("ListeningLesson")
                .ToListAsync();
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        public async Task<ListeningQuestion?>
            GetListeningQuestionByIdAsync(
                int id)
        {
            var query =
                from item in _context.ListeningQuestions
                where item.Id == id
                select item;

            return await query
                .Include("ListeningLesson")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // CREATE
        // ==========================================

        public async Task CreateListeningQuestionAsync(
            ListeningQuestion listeningQuestion)
        {
            listeningQuestion.Question =
                listeningQuestion.Question.Trim();

            listeningQuestion.Answer =
                listeningQuestion.Answer.Trim();

            _context.ListeningQuestions.Add(
                listeningQuestion);

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // UPDATE
        // ==========================================

        public async Task UpdateListeningQuestionAsync(
            ListeningQuestion listeningQuestion)
        {
            var existing =
                await GetListeningQuestionByIdAsync(
                    listeningQuestion.Id);

            if (existing == null)
                return;

            existing.ListeningLessonId =
                listeningQuestion.ListeningLessonId;

            existing.Question =
                listeningQuestion.Question.Trim();

            existing.Answer =
                listeningQuestion.Answer.Trim();

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // DELETE
        // ==========================================

        public async Task<bool>
            DeleteListeningQuestionAsync(
                int id)
        {
            var listeningQuestion =
                await GetListeningQuestionByIdAsync(id);

            if (listeningQuestion == null)
                return false;

            _context.ListeningQuestions.Remove(
                listeningQuestion);

            await _context.SaveChangesAsync();

            return true;
        }

        // ==========================================
        // CHECK DUPLICATE
        // ==========================================

        public async Task<bool>
            IsQuestionExistsAsync(
                int listeningLessonId,
                string question,
                int? id = null)
        {
            string normalizedQuestion =
                question.Trim().ToLower();

            var query =
                from item in _context.ListeningQuestions
                where item.ListeningLessonId ==
                      listeningLessonId
                      &&
                      item.Question
                          .ToLower()
                          == normalizedQuestion
                select item;

            if (id.HasValue)
            {
                query =
                    from item in query
                    where item.Id != id.Value
                    select item;
            }

            return await query.AnyAsync();
        }
    }
}