using AppLearningEnglish.Business.IService;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Service
{
    public class AnswerService : IAnswerService
    {
        private readonly ApplicationDbContext _context;

        public AnswerService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL
        // ==========================================

        public async Task<IEnumerable<Answer>>
            GetAllAnswerAsync()
        {
            var query =
                from answer in _context.answers
                orderby answer.QuestionId,
                         answer.Id
                select answer;

            return await query
                .Include("Question")
                .ToListAsync();
        }

        // ==========================================
        // SEARCH + FILTER
        // ==========================================

        public async Task<IEnumerable<Answer>>
            SearchAnswerAsync(
                string? search,
                int? questionId)
        {
            IQueryable<Answer> query =
                _context.answers;

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query =
                    from answer in query
                    where
                        answer.AnswerText
                            .ToLower()
                            .Contains(keyword)
                        ||
                        (
                            answer.Question != null
                            &&
                            answer.Question.QuestionText
                                .ToLower()
                                .Contains(keyword)
                        )
                    select answer;
            }

            // FILTER QUESTION
            if (questionId.HasValue &&
                questionId.Value > 0)
            {
                int selectedQuestionId =
                    questionId.Value;

                query =
                    from answer in query
                    where answer.QuestionId ==
                          selectedQuestionId
                    select answer;
            }

            query =
                from answer in query
                orderby answer.QuestionId,
                         answer.Id
                select answer;

            return await query
                .Include("Question")
                .ToListAsync();
        }

        // ==========================================
        // GET BY QUESTION
        // ==========================================

        public async Task<IEnumerable<Answer>>
            GetByQuestionIdAsync(
                int questionId)
        {
            var query =
                from answer in _context.answers
                where answer.QuestionId == questionId
                orderby answer.Id
                select answer;

            return await query
                .Include("Question")
                .ToListAsync();
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        public async Task<Answer?>
            GetAnswerByIdAsync(int id)
        {
            var query =
                from answer in _context.answers
                where answer.Id == id
                select answer;

            return await query
                .Include("Question")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // CREATE
        // ==========================================

        public async Task CreateAnswerAsync(
            Answer answer)
        {
            answer.AnswerText =
                answer.AnswerText.Trim();

            // Nếu Answer mới là đáp án đúng
            // thì tất cả Answer khác của Question
            // phải chuyển thành false.

            if (answer.IsCorrect)
            {
                var otherAnswers =
                    from item in _context.answers
                    where item.QuestionId ==
                          answer.QuestionId
                    select item;

                var list =
                    await otherAnswers.ToListAsync();

                foreach (var item in list)
                {
                    item.IsCorrect = false;
                }
            }

            _context.answers.Add(answer);

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // UPDATE
        // ==========================================

        public async Task UpdateAnswerAsync(
            Answer answer)
        {
            var existingAnswer =
                await GetAnswerByIdAsync(
                    answer.Id);

            if (existingAnswer == null)
                return;

            existingAnswer.QuestionId =
                answer.QuestionId;

            existingAnswer.AnswerText =
                answer.AnswerText.Trim();

            existingAnswer.IsCorrect =
                answer.IsCorrect;

            // Nếu Answer đang được chọn là đúng
            // thì các Answer khác phải false.

            if (answer.IsCorrect)
            {
                var otherAnswers =
                    from item in _context.answers
                    where item.QuestionId ==
                          answer.QuestionId
                          &&
                          item.Id != answer.Id
                    select item;

                var list =
                    await otherAnswers.ToListAsync();

                foreach (var item in list)
                {
                    item.IsCorrect = false;
                }
            }

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // DELETE
        // ==========================================

        public async Task<bool> DeleteAnswerAsync(
            int id)
        {
            var answer =
                await GetAnswerByIdAsync(id);

            if (answer == null)
                return false;

            _context.answers.Remove(answer);

            await _context.SaveChangesAsync();

            return true;
        }

        // ==========================================
        // CHECK DUPLICATE
        // ==========================================

        public async Task<bool> IsAnswerExistsAsync(
            int questionId,
            string answerText,
            int? id = null)
        {
            string normalizedAnswer =
                answerText
                    .Trim()
                    .ToLower();

            var query =
                from answer in _context.answers
                where answer.QuestionId == questionId
                      &&
                      answer.AnswerText
                          .ToLower()
                          == normalizedAnswer
                select answer;

            if (id.HasValue)
            {
                query =
                    from answer in query
                    where answer.Id != id.Value
                    select answer;
            }

            return await query.AnyAsync();
        }

        // ==========================================
        // SET CORRECT
        // ==========================================

        public async Task SetCorrectAnswerAsync(
            int id)
        {
            var answer =
                await GetAnswerByIdAsync(id);

            if (answer == null)
                return;

            var otherAnswers =
                from item in _context.answers
                where item.QuestionId ==
                      answer.QuestionId
                      &&
                      item.Id != answer.Id
                select item;

            var list =
                await otherAnswers.ToListAsync();

            foreach (var item in list)
            {
                item.IsCorrect = false;
            }

            answer.IsCorrect = true;

            await _context.SaveChangesAsync();
        }

        // ==========================================
        // SET INCORRECT
        // ==========================================

        public async Task SetIncorrectAnswerAsync(
            int id)
        {
            var answer =
                await GetAnswerByIdAsync(id);

            if (answer == null)
                return;

            answer.IsCorrect = false;

            await _context.SaveChangesAsync();
        }
    }
}