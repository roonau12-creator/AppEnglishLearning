using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business.Services.IServices
{
    public interface ILearningAccessService
    {
        Task<bool> IsAdminBypassAsync(string userId);

        Task<bool> IsEnrolledForCourseAsync(string userId, int courseId);

        Task<bool> IsEnrolledForLessonAsync(string userId, int lessonId);

        Task<Lesson?> GetPublishedLessonAsync(int lessonId);

        Task<bool> CanAccessCourseLevelAsync(string userId, string? courseLevel);

        Task<string?> GetUserLevelAsync(string userId);
    }
}
