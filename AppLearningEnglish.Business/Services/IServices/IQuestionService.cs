using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IQuestionService
    {
        Task<IEnumerable<Question>> GetAllQuestionAsync();

        Task<IEnumerable<Question>> SearchQuestionAsync(
            string? search,
            int? exerciseId);

        Task<IEnumerable<Question>> GetByExerciseIdAsync(
            int exerciseId);

        Task<Question?> GetQuestionByIdAsync(
            int id);

        Task CreateQuestionAsync(
            Question question);

        Task UpdateQuestionAsync(
            Question question);

        Task<bool> DeleteQuestionAsync(
            int id);

        Task<bool> IsQuestionExistsAsync(
            int exerciseId,
            string questionText,
            int? id = null);
    }
}