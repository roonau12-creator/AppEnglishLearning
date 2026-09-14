using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business.Services.IServices
{
     public interface IExerciseAttemptService
    {
        Task<ExerciseAttempt?> GetByIdAsync(
            int id,
            string userId);

        Task<IEnumerable<ExerciseAttempt>> GetAllAsync(
            string userId);

        Task<IEnumerable<ExerciseAttempt>> GetByExerciseAsync(
            string userId,
            int exerciseId);

        Task<ExerciseAttempt?> GetLatestAsync(
            string userId,
            int exerciseId);

        Task<ExerciseAttempt> StartAsync(
            string userId,
            int exerciseId,
            int totalQuestions);

        Task<ExerciseAttempt?> SubmitAsync(
            int attemptId,
            string userId,
            int correctAnswers,
            string? selectedAnswersJson = null,
            string? textAnswersJson = null);

        Task<decimal> GetBestScoreAsync(
            string userId,
            int exerciseId);

        Task<int> GetTotalAttemptsAsync(
            string userId,
            int exerciseId);
    }
}