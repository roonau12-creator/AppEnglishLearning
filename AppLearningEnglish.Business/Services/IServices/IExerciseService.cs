using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
   public interface IExerciseService
    {
        Task<IEnumerable<Exercise>> GetAllExerciseAsync();

        Task<IEnumerable<Exercise>> SearchExerciseAsync(
            string? search,
            int? lessonId,
            string? type);

        Task<IEnumerable<Exercise>> GetByLessonIdAsync(
            int lessonId);

        Task<Exercise?> GetExerciseByIdAsync(
            int id);

        Task CreateExerciseAsync(
            Exercise exercise);

        Task UpdateExerciseAsync(
            Exercise exercise);

        Task<bool> DeleteExerciseAsync(
            int id);

        Task<bool> IsQuestionExistsAsync(
            int lessonId,
            string question,
            int? id = null);
    }
}