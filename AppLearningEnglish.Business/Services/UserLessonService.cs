using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore; 
namespace AppLearningEnglish.Business.Services
{
    public class UserLessonService : IUserLessonService
    {
        private readonly ApplicationDbContext _context;

        public UserLessonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserLesson?> GetUserLessonAsync(
            string userId,
            int lessonId)
        {
            var query =
                from userLesson in _context.UserLessons
                where userLesson.ApplicationUserId == userId
                      && userLesson.LessonId == lessonId
                select userLesson;

            return await query
                .Include("Lesson")
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserLesson>> GetUserLessonsAsync(
            string userId)
        {
            var query =
                from userLesson in _context.UserLessons
                where userLesson.ApplicationUserId== userId
                orderby userLesson.LessonId
                select userLesson;

            return await query
                .Include("Lesson")
                .Include("Lesson.Course")
                .ToListAsync();
        }

        public async Task<IEnumerable<UserLesson>> GetCompletedLessonsAsync(
            string userId)
        {
            var query =
                from userLesson in _context.UserLessons
                where userLesson.ApplicationUserId == userId
                      && userLesson.IsCompleted
                orderby userLesson.CompletedAt descending
                select userLesson;

            return await query
                .Include("Lesson")
                .Include("Lesson.Course")
                .ToListAsync();
        }

        public async Task<IEnumerable<UserLesson>> GetInProgressLessonsAsync(
            string userId)
        {
            var query =
                from userLesson in _context.UserLessons
                where userLesson.ApplicationUserId == userId
                      && !userLesson.IsCompleted
                      && userLesson.Progress > 0
                orderby userLesson.Progress descending
                select userLesson;

            return await query
                .Include("Lesson")
                .Include("Lesson.Course")
                .ToListAsync();
        }

        public async Task StartLessonAsync(
            string userId,
            int lessonId)
        {
            var userLesson =
                await GetUserLessonAsync(userId, lessonId);

            if (userLesson != null)
            {
                return;
            }

            var lesson = await _context.Lessons
                .FindAsync(lessonId);

            if (lesson == null)
            {
                return;
            }

            var newUserLesson = new UserLesson
            {
                ApplicationUserId = userId,
                LessonId = lessonId,
                Progress = 0,
                IsCompleted = false,
                CompletedAt = null
            };

            _context.UserLessons.Add(newUserLesson);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateProgressAsync(
            string userId,
            int lessonId,
            decimal progress)
        {
            if (progress < 0)
            {
                progress = 0;
            }

            if (progress > 100)
            {
                progress = 100;
            }

            var userLesson =
                await GetUserLessonAsync(userId, lessonId);

            if (userLesson == null)
            {
                userLesson = new UserLesson
                {
                    ApplicationUserId = userId,
                    LessonId = lessonId,
                    Progress = progress,
                    IsCompleted = false
                };

                _context.UserLessons.Add(userLesson);
            }
            else
            {
                userLesson.Progress = progress;

                if (progress >= 100)
                {
                    userLesson.Progress = 100;
                    userLesson.IsCompleted = true;
                    userLesson.CompletedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task EnsureMinProgressAsync(
            string userId,
            int lessonId,
            decimal minProgress)
        {
            if (minProgress < 0)
            {
                minProgress = 0;
            }

            if (minProgress > 100)
            {
                minProgress = 100;
            }

            await StartLessonAsync(userId, lessonId);

            var userLesson =
                await GetUserLessonAsync(userId, lessonId);

            if (userLesson == null || userLesson.IsCompleted)
            {
                return;
            }

            if (userLesson.Progress >= minProgress)
            {
                return;
            }

            userLesson.Progress = minProgress;

            if (minProgress >= 100)
            {
                userLesson.IsCompleted = true;
                userLesson.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CompleteLessonAsync(
            string userId,
            int lessonId)
        {
            var userLesson =
                await GetUserLessonAsync(userId, lessonId);

            if (userLesson == null)
            {
                userLesson = new UserLesson
                {
                    ApplicationUserId = userId,
                    LessonId = lessonId
                };

                _context.UserLessons.Add(userLesson);
            }
            else if (userLesson.IsCompleted)
            {
                return false;
            }

            userLesson.Progress = 100;
            userLesson.IsCompleted = true;
            userLesson.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task MarkVocabDoneAsync(string userId, int lessonId)
        {
            await MarkStepAsync(userId, lessonId, vocab: true);
        }

        public async Task MarkListeningDoneAsync(string userId, int lessonId)
        {
            await MarkStepAsync(userId, lessonId, listening: true);
        }

        public async Task MarkExerciseDoneAsync(string userId, int lessonId)
        {
            await MarkStepAsync(userId, lessonId, exercise: true);
        }

        public async Task MarkReadingDoneAsync(string userId, int lessonId)
        {
            await MarkStepAsync(userId, lessonId, reading: true);
        }

        public async Task MarkSpeakingDoneAsync(string userId, int lessonId)
        {
            await MarkStepAsync(userId, lessonId, speaking: true);
        }

        public async Task MarkWritingDoneAsync(string userId, int lessonId)
        {
            await MarkStepAsync(userId, lessonId, writing: true);
        }

        public async Task MarkGrammarDoneAsync(string userId, int lessonId)
        {
            await MarkStepAsync(userId, lessonId, grammar: true);
        }

        public async Task MarkReadingForPassageAsync(string userId, int passageId)
        {
            var ids = await _context.Lessons
                .Where(x => x.ReadingPassageId == passageId)
                .Select(x => x.Id)
                .ToListAsync();
            foreach (var id in ids)
            {
                await MarkReadingDoneAsync(userId, id);
            }
        }

        public async Task MarkGrammarForTopicAsync(string userId, int topicId)
        {
            var ids = await _context.Lessons
                .Where(x => x.GrammarTopicId == topicId)
                .Select(x => x.Id)
                .ToListAsync();
            foreach (var id in ids)
            {
                await MarkGrammarDoneAsync(userId, id);
            }
        }

        public async Task MarkWritingForPromptAsync(string userId, int promptId)
        {
            var ids = await _context.Lessons
                .Where(x => x.WritingPromptId == promptId)
                .Select(x => x.Id)
                .ToListAsync();
            foreach (var id in ids)
            {
                await MarkWritingDoneAsync(userId, id);
            }
        }

        private async Task MarkStepAsync(
            string userId,
            int lessonId,
            bool vocab = false,
            bool listening = false,
            bool exercise = false,
            bool reading = false,
            bool speaking = false,
            bool writing = false,
            bool grammar = false)
        {
            await StartLessonAsync(userId, lessonId);

            var userLesson = await GetUserLessonAsync(userId, lessonId);
            if (userLesson == null)
            {
                return;
            }

            if (vocab)
            {
                userLesson.VocabDone = true;
            }

            if (listening)
            {
                userLesson.ListeningDone = true;
            }

            if (exercise)
            {
                userLesson.ExerciseDone = true;
            }

            if (reading)
            {
                userLesson.ReadingDone = true;
            }

            if (speaking)
            {
                userLesson.SpeakingDone = true;
            }

            if (writing)
            {
                userLesson.WritingDone = true;
            }

            if (grammar)
            {
                userLesson.GrammarDone = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetCourseProgressAsync(
            string userId,
            int courseId)
        {
            var lessonsQuery =
                from lesson in _context.Lessons
                where lesson.CourseId == courseId
                      && lesson.IsPublished
                select lesson;

            var totalLessons =
                await lessonsQuery.CountAsync();

            if (totalLessons == 0)
            {
                return 0;
            }

            var completedQuery =
                from userLesson in _context.UserLessons
                join lesson in _context.Lessons
                    on userLesson.LessonId equals lesson.Id
                where userLesson.ApplicationUserId == userId
                      && lesson.CourseId == courseId
                      && lesson.IsPublished
                      && userLesson.IsCompleted
                select userLesson;

            var completedLessons =
                await completedQuery.CountAsync();

            decimal progress =
                (decimal)completedLessons / totalLessons * 100;

            return Math.Round(progress, 2);
        }
    }
}