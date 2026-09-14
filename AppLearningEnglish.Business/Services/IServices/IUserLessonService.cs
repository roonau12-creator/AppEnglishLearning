using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
     public interface IUserLessonService
    {
        Task<UserLesson?> GetUserLessonAsync(
            string userId,
            int lessonId);

        Task<IEnumerable<UserLesson>> GetUserLessonsAsync(
            string userId);

        Task<IEnumerable<UserLesson>> GetCompletedLessonsAsync(
            string userId);

        Task<IEnumerable<UserLesson>> GetInProgressLessonsAsync(
            string userId);

        Task StartLessonAsync(
            string userId,
            int lessonId);

        Task UpdateProgressAsync(
            string userId,
            int lessonId,
            decimal progress);

        Task EnsureMinProgressAsync(
            string userId,
            int lessonId,
            decimal minProgress);

        Task<bool> CompleteLessonAsync(
            string userId,
            int lessonId);

        Task MarkVocabDoneAsync(string userId, int lessonId);

        Task MarkListeningDoneAsync(string userId, int lessonId);

        Task MarkExerciseDoneAsync(string userId, int lessonId);

        Task MarkReadingDoneAsync(string userId, int lessonId);

        Task MarkSpeakingDoneAsync(string userId, int lessonId);

        Task MarkWritingDoneAsync(string userId, int lessonId);

        Task MarkGrammarDoneAsync(string userId, int lessonId);

        Task MarkReadingForPassageAsync(string userId, int passageId);

        Task MarkGrammarForTopicAsync(string userId, int topicId);

        Task MarkWritingForPromptAsync(string userId, int promptId);

        Task<decimal> GetCourseProgressAsync(
            string userId,
            int courseId);
    }
}