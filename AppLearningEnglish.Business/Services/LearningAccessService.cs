using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Business;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Services
{
    public class LearningAccessService : ILearningAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LearningAccessService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> IsAdminBypassAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, AppRoles.Admin);
        }

        public async Task<bool> IsEnrolledForCourseAsync(string userId, int courseId)
        {
            if (await IsAdminBypassAsync(userId))
            {
                return true;
            }

            return await _context.UserCourses.AnyAsync(x =>
                x.ApplicationUserId == userId && x.CourseId == courseId);
        }

        public async Task<bool> IsEnrolledForLessonAsync(string userId, int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null)
            {
                return false;
            }

            return await IsEnrolledForCourseAsync(userId, lesson.CourseId);
        }

        public async Task<Lesson?> GetPublishedLessonAsync(int lessonId)
        {
            return await _context.Lessons
                .Include("Course")
                .Include("Topic")
                .FirstOrDefaultAsync(x => x.Id == lessonId && x.IsPublished);
        }

        public async Task<bool> CanAccessCourseLevelAsync(string userId, string? courseLevel)
        {
            if (await IsAdminBypassAsync(userId))
            {
                return true;
            }

            var user = await _userManager.FindByIdAsync(userId);
            return !EnglishLevelScale.IsTooHard(user?.EnglishLevel, courseLevel);
        }

        public async Task<string?> GetUserLevelAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.EnglishLevel;
        }
    }
}
