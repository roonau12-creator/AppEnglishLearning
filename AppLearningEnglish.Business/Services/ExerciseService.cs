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
     public class ExerciseService : IExerciseService
    {
        private readonly ApplicationDbContext _context;

        public ExerciseService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET ALL
        // =========================================

        public async Task<IEnumerable<Exercise>>
            GetAllExerciseAsync()
        {
            var query =
                from exercise in _context.exercises
                orderby exercise.LessonId,
                         exercise.ExerciseOrder
                select exercise;

            return await query
                .Include("Lesson")
                .ToListAsync();
        }

        // =========================================
        // SEARCH + FILTER
        // =========================================

        public async Task<IEnumerable<Exercise>>
            SearchExerciseAsync(
                string? search,
                int? lessonId,
                string? type)
        {
            IQueryable<Exercise> query =
                _context.exercises;

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query =
                    from exercise in query
                    where
                        exercise.Question
                            .ToLower()
                            .Contains(keyword)
                        ||
                        exercise.Type
                            .ToLower()
                            .Contains(keyword)
                        ||
                        (
                            exercise.Lesson != null
                            &&
                            exercise.Lesson.Title
                                .ToLower()
                                .Contains(keyword)
                        )
                    select exercise;
            }

            // FILTER LESSON
            if (lessonId.HasValue &&
                lessonId.Value > 0)
            {
                int selectedLessonId =
                    lessonId.Value;

                query =
                    from exercise in query
                    where exercise.LessonId ==
                          selectedLessonId
                    select exercise;
            }

            // FILTER TYPE
            if (!string.IsNullOrWhiteSpace(type))
            {
                string selectedType =
                    type.Trim();

                query =
                    from exercise in query
                    where exercise.Type ==
                          selectedType
                    select exercise;
            }

            query =
                from exercise in query
                orderby exercise.LessonId,
                         exercise.ExerciseOrder
                select exercise;

            return await query
                .Include("Lesson")
                .ToListAsync();
        }

        // =========================================
        // GET BY LESSON
        // =========================================

        public async Task<IEnumerable<Exercise>>
            GetByLessonIdAsync(int lessonId)
        {
            var query =
                from exercise in _context.exercises
                where exercise.LessonId == lessonId
                orderby exercise.ExerciseOrder
                select exercise;

            return await query
                .Include("Lesson")
                .ToListAsync();
        }

        // =========================================
        // GET BY ID
        // =========================================

        public async Task<Exercise?>
            GetExerciseByIdAsync(int id)
        {
            var query =
                from exercise in _context.exercises
                where exercise.Id == id
                select exercise;

            return await query
                .Include("Lesson")
                .FirstOrDefaultAsync();
        }

        // =========================================
        // CREATE
        // =========================================

        public async Task CreateExerciseAsync(
            Exercise exercise)
        {
            exercise.Type =
                exercise.Type.Trim();

            exercise.Question =
                exercise.Question.Trim();

            _context.exercises.Add(exercise);

            await _context.SaveChangesAsync();
        }

        // =========================================
        // UPDATE
        // =========================================

        public async Task UpdateExerciseAsync(
            Exercise exercise)
        {
            var existingExercise =
                await GetExerciseByIdAsync(
                    exercise.Id);

            if (existingExercise == null)
                return;

            existingExercise.LessonId =
                exercise.LessonId;

            existingExercise.Type =
                exercise.Type.Trim();

            existingExercise.Question =
                exercise.Question.Trim();

            existingExercise.ExerciseOrder =
                exercise.ExerciseOrder;

            await _context.SaveChangesAsync();
        }

        // =========================================
        // DELETE
        // =========================================

        public async Task<bool> DeleteExerciseAsync(
            int id)
        {
            var exercise =
                await GetExerciseByIdAsync(id);

            if (exercise == null)
                return false;

            _context.exercises.Remove(exercise);

            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================
        // CHECK DUPLICATE QUESTION
        // =========================================

        public async Task<bool> IsQuestionExistsAsync(
            int lessonId,
            string question,
            int? id = null)
        {
            string normalizedQuestion =
                question.Trim().ToLower();

            var query =
                from exercise in _context.exercises
                where exercise.LessonId == lessonId
                      &&
                      exercise.Question
                          .ToLower()
                          == normalizedQuestion
                select exercise;

            if (id.HasValue)
            {
                query =
                    from exercise in query
                    where exercise.Id != id.Value
                    select exercise;
            }

            return await query.AnyAsync();
        }
    }
}