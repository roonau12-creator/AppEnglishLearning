using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
namespace AppLearningEnglish.Business.Services
{
    public class ExerciseAttemptService
        : IExerciseAttemptService
    {
        private readonly ApplicationDbContext _context;

        public ExerciseAttemptService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET BY ID
        // ==========================================

        public async Task<ExerciseAttempt?> GetByIdAsync(
            int id,
            string userId)
        {
            var query =
                from attempt in _context.ExerciseAttempts
                where attempt.Id == id
                      && attempt.ApplicationUserId == userId
                select attempt;

            return await query
                .Include("Exercise")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // GET ALL ATTEMPTS
        // ==========================================

        public async Task<IEnumerable<ExerciseAttempt>>
            GetAllAsync(string userId)
        {
            var query =
                from attempt in _context.ExerciseAttempts
                where attempt.ApplicationUserId == userId
                orderby attempt.StartedAt descending
                select attempt;

            return await query
                .Include("Exercise")
                .ToListAsync();
        }

        // ==========================================
        // GET ATTEMPTS BY EXERCISE
        // ==========================================

        public async Task<IEnumerable<ExerciseAttempt>>
            GetByExerciseAsync(
                string userId,
                int exerciseId)
        {
            var query =
                from attempt in _context.ExerciseAttempts
                where attempt.ApplicationUserId == userId
                      && attempt.ExerciseId == exerciseId
                orderby attempt.StartedAt descending
                select attempt;

            return await query
                .Include("Exercise")
                .ToListAsync();
        }

        // ==========================================
        // GET LATEST
        // ==========================================

        public async Task<ExerciseAttempt?> GetLatestAsync(
            string userId,
            int exerciseId)
        {
            var query =
                from attempt in _context.ExerciseAttempts
                where attempt.ApplicationUserId == userId
                      && attempt.ExerciseId == exerciseId
                orderby attempt.StartedAt descending
                select attempt;

            return await query
                .Include("Exercise")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // START
        // ==========================================

        public async Task<ExerciseAttempt> StartAsync(
            string userId,
            int exerciseId,
            int totalQuestions)
        {
            var exercise =
                await _context.exercises
                    .FindAsync(exerciseId);

            if (exercise == null)
            {
                throw new Exception(
                    "Exercise không tồn tại.");
            }

            var attempt = new ExerciseAttempt
            {
                ApplicationUserId = userId,
                ExerciseId = exerciseId,
                Score = 0,
                CorrectAnswers = 0,
                TotalQuestions = totalQuestions,
                StartedAt = DateTime.UtcNow,
                CompletedAt = null
            };

            _context.ExerciseAttempts.Add(attempt);

            await _context.SaveChangesAsync();

            return attempt;
        }

        // ==========================================
        // SUBMIT
        // ==========================================

        public async Task<ExerciseAttempt?> SubmitAsync(
            int attemptId,
            string userId,
            int correctAnswers,
            string? selectedAnswersJson = null,
            string? textAnswersJson = null)
        {
            var query =
                from item in _context.ExerciseAttempts
                where item.Id == attemptId
                      && item.ApplicationUserId == userId
                select item;

            var attempt =
                await query.FirstOrDefaultAsync();

            if (attempt == null)
            {
                return null;
            }

            if (attempt.CompletedAt != null)
            {
                return attempt;
            }

            if (correctAnswers < 0)
            {
                correctAnswers = 0;
            }

            if (correctAnswers >
                attempt.TotalQuestions)
            {
                correctAnswers =
                    attempt.TotalQuestions;
            }

            attempt.CorrectAnswers =
                correctAnswers;

            if (attempt.TotalQuestions > 0)
            {
                attempt.Score =
                    Math.Round(
                        (decimal)correctAnswers
                        / attempt.TotalQuestions
                        * 100,
                        2);
            }
            else
            {
                attempt.Score = 0;
            }

            attempt.CompletedAt =
                DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(selectedAnswersJson))
            {
                attempt.SelectedAnswersJson = selectedAnswersJson;
            }

            if (!string.IsNullOrWhiteSpace(textAnswersJson))
            {
                attempt.TextAnswersJson = textAnswersJson;
            }

            await _context.SaveChangesAsync();

            return attempt;
        }

        // ==========================================
        // BEST SCORE
        // ==========================================

        public async Task<decimal> GetBestScoreAsync(
            string userId,
            int exerciseId)
        {
            var query =
                from attempt in _context.ExerciseAttempts
                where attempt.ApplicationUserId == userId
                      && attempt.ExerciseId == exerciseId
                      && attempt.CompletedAt != null
                select attempt.Score;

            var scores =
                await query.ToListAsync();

            if (scores.Count == 0)
            {
                return 0;
            }

            decimal bestScore = 0;

            foreach (var score in scores)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                }
            }

            return bestScore;
        }

        // ==========================================
        // TOTAL ATTEMPTS
        // ==========================================

        public async Task<int> GetTotalAttemptsAsync(
            string userId,
            int exerciseId)
        {
            var query =
                from attempt in _context.ExerciseAttempts
                where attempt.ApplicationUserId == userId
                      && attempt.ExerciseId == exerciseId
                select attempt;

            return await query.CountAsync();
        }
    }
}