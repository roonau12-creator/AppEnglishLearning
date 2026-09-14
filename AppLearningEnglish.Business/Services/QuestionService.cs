using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Service
{
    public class QuestionService : IQuestionService
    {
        private readonly ApplicationDbContext _context;

        public QuestionService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL
        // ==========================================

        public async Task<IEnumerable<Question>>
            GetAllQuestionAsync()
        {
            var query =
                from question in _context.questions
                orderby question.ExerciseId,
                         question.Id
                select question;

            return await query
                .Include("Exercise")
                .ToListAsync();
        }

        // ==========================================
        // SEARCH
        // ==========================================

        public async Task<IEnumerable<Question>>
            SearchQuestionAsync(
                string? search,
                int? exerciseId)
        {
            IQueryable<Question> query =
                _context.questions;

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query =
                    from question in query
                    where
                        question.QuestionText
                            .ToLower()
                            .Contains(keyword)
                        ||
                        (
                            question.Explanation != null
                            &&
                            question.Explanation
                                .ToLower()
                                .Contains(keyword)
                        )
                        ||
                        (
                            question.Exercise != null
                            &&
                            question.Exercise.Type
                                .ToLower()
                                .Contains(keyword)
                        )
                    select question;
            }

            // FILTER EXERCISE
            if (exerciseId.HasValue &&
                exerciseId.Value > 0)
            {
                int selectedExerciseId =
                    exerciseId.Value;

                query =
                    from question in query
                    where question.ExerciseId ==
                          selectedExerciseId
                    select question;
            }

            query =
                from question in query
                orderby question.ExerciseId,
                         question.Id
                select question;

            return await query
                .Include("Exercise")
                .ToListAsync();
        }

        // ==========================================
        // GET BY EXERCISE
        // ==========================================

        public async Task<IEnumerable<Question>>
            GetByExerciseIdAsync(
                int exerciseId)
        {
            var query =
                from question in _context.questions
                where question.ExerciseId == exerciseId
                orderby question.Id
                select question;

            return await query
                .Include("Exercise")
                .ToListAsync();
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        public async Task<Question?>
            GetQuestionByIdAsync(int id)
        {
            var query =
                from question in _context.questions
                where question.Id == id
                select question;

            return await query
                .Include("Exercise")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // CREATE
        // ==========================================

        public async Task CreateQuestionAsync(
            Question question)
        {
            question.QuestionText =
                question.QuestionText.Trim();

            if (!string.IsNullOrWhiteSpace(
                question.Explanation))
            {
                question.Explanation =
                    question.Explanation.Trim();
            }

            _context.questions.Add(question);

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // UPDATE
        // ==========================================

        public async Task UpdateQuestionAsync(
            Question question)
        {
            var existingQuestion =
                await GetQuestionByIdAsync(
                    question.Id);

            if (existingQuestion == null)
                return;

            existingQuestion.ExerciseId =
                question.ExerciseId;

            existingQuestion.QuestionText =
                question.QuestionText.Trim();

            if (!string.IsNullOrWhiteSpace(
                question.Explanation))
            {
                existingQuestion.Explanation =
                    question.Explanation.Trim();
            }
            else
            {
                existingQuestion.Explanation = null;
            }

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // DELETE
        // ==========================================

        public async Task<bool> DeleteQuestionAsync(
            int id)
        {
            var question =
                await GetQuestionByIdAsync(id);

            if (question == null)
                return false;

            _context.questions.Remove(question);

            await _context.SaveChangesAsync();

            return true;
        }

        // ==========================================
        // CHECK DUPLICATE
        // ==========================================

        public async Task<bool> IsQuestionExistsAsync(
            int exerciseId,
            string questionText,
            int? id = null)
        {
            string normalizedQuestion =
                questionText
                    .Trim()
                    .ToLower();

            var query =
                from question in _context.questions
                where question.ExerciseId == exerciseId
                      &&
                      question.QuestionText
                          .ToLower()
                          == normalizedQuestion
                select question;

            if (id.HasValue)
            {
                query =
                    from question in query
                    where question.Id != id.Value
                    select question;
            }

            return await query.AnyAsync();
        }
    }
}