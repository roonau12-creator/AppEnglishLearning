using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
     public interface IUserCourseService
    {
        Task<UserCourse?> GetAsync(
            string userId,
            int courseId);

        Task<IEnumerable<UserCourse>> GetMyCoursesAsync(
            string userId);

        Task<IEnumerable<UserCourse>> GetInProgressCoursesAsync(
            string userId);

        Task<IEnumerable<UserCourse>> GetCompletedCoursesAsync(
            string userId);

        Task<bool> IsEnrolledAsync(
            string userId,
            int courseId);

        Task<bool> EnrollAsync(
            string userId,
            int courseId);

        Task<bool> RemoveEnrollmentAsync(
            string userId,
            int courseId);

        Task<decimal> GetProgressAsync(
            string userId,
            int courseId);

        Task<UserCourse?> GetByCertificateCodeAsync(string code);

        Task<bool> UpdateProgressAsync(
            string userId,
            int courseId);

        Task<bool> CompleteCourseAsync(
            string userId,
            int courseId);

        Task<Lesson?> GetContinueLessonAsync(
            string userId,
            int courseId);

        Task<int> GetTotalCoursesAsync(
            string userId);

        Task<int> GetCompletedCoursesCountAsync(
            string userId);

        Task<int> GetInProgressCoursesCountAsync(
            string userId);
    }
}