using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Business;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;
namespace AppLearningEnglish.Business.Services
{
    public class UserCourseService : IUserCourseService
    {
        private readonly ApplicationDbContext _context;

        public UserCourseService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET USER COURSE
        // ==========================================

        public async Task<UserCourse?> GetAsync(
            string userId,
            int courseId)
        {
            var query =
                from item in _context.UserCourses
                where item.ApplicationUserId == userId
                      && item.CourseId == courseId
                select item;

            return await query
                .Include("Course")
                .FirstOrDefaultAsync();
        }

        // ==========================================
        // MY COURSES
        // ==========================================

        public async Task<IEnumerable<UserCourse>>
            GetMyCoursesAsync(string userId)
        {
            var query =
                from item in _context.UserCourses
                where item.ApplicationUserId == userId
                orderby item.EnrolledAt descending
                select item;

            return await query
                .Include("Course")
                .ToListAsync();
        }

        // ==========================================
        // IN PROGRESS
        // ==========================================

        public async Task<IEnumerable<UserCourse>>
            GetInProgressCoursesAsync(string userId)
        {
            var query =
                from item in _context.UserCourses
                where item.ApplicationUserId == userId
                      && item.Progress < 100
                orderby item.EnrolledAt descending
                select item;

            return await query
                .Include("Course")
                .ToListAsync();
        }

        // ==========================================
        // COMPLETED
        // ==========================================

        public async Task<IEnumerable<UserCourse>>
            GetCompletedCoursesAsync(string userId)
        {
            var query =
                from item in _context.UserCourses
                where item.ApplicationUserId == userId
                      && item.Progress >= 100
                orderby item.CompletedAt descending
                select item;

            return await query
                .Include("Course")
                .ToListAsync();
        }

        // ==========================================
        // CHECK ENROLLED
        // ==========================================

        public async Task<bool> IsEnrolledAsync(
            string userId,
            int courseId)
        {
            var query =
                from item in _context.UserCourses
                where item.ApplicationUserId == userId
                      && item.CourseId == courseId
                select item;

            return await query.AnyAsync();
        }

        // ==========================================
        // ENROLL
        // ==========================================

        public async Task<bool> EnrollAsync(
            string userId,
            int courseId)
        {
            var course =
                await _context.courses.FindAsync(courseId);

            if (course == null)
            {
                return false;
            }

            if (!course.IsPublished)
            {
                return false;
            }

            bool enrolled =
                await IsEnrolledAsync(
                    userId,
                    courseId);

            if (enrolled)
            {
                return false;
            }

            var userCourse =
                new UserCourse
                {
                    ApplicationUserId = userId,
                    CourseId = courseId,
                    Progress = 0,
                    EnrolledAt = DateTime.UtcNow,
                    CompletedAt = null
                };

            _context.UserCourses.Add(userCourse);

            await _context.SaveChangesAsync();

            return true;
        }

        // ==========================================
        // REMOVE ENROLLMENT
        // ==========================================

        public async Task<bool> RemoveEnrollmentAsync(
            string userId,
            int courseId)
        {
            var userCourse =
                await GetAsync(
                    userId,
                    courseId);

            if (userCourse == null)
            {
                return false;
            }

            _context.UserCourses.Remove(userCourse);

            var lessonIds = await _context.Lessons
                .Where(x => x.CourseId == courseId)
                .Select(x => x.Id)
                .ToListAsync();

            if (lessonIds.Count > 0)
            {
                var userLessons = await _context.UserLessons
                    .Where(x => x.ApplicationUserId == userId && lessonIds.Contains(x.LessonId))
                    .ToListAsync();
                _context.UserLessons.RemoveRange(userLessons);

                var exerciseIds = await _context.exercises
                    .Where(x => lessonIds.Contains(x.LessonId))
                    .Select(x => x.Id)
                    .ToListAsync();

                if (exerciseIds.Count > 0)
                {
                    var attempts = await _context.ExerciseAttempts
                        .Where(x => x.ApplicationUserId == userId && exerciseIds.Contains(x.ExerciseId))
                        .ToListAsync();
                    _context.ExerciseAttempts.RemoveRange(attempts);
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }

        // ==========================================
        // CALCULATE PROGRESS
        // ==========================================

        public async Task<decimal> GetProgressAsync(
            string userId,
            int courseId)
        {
            var lessonsQuery =
                from lesson in _context.Lessons
                where lesson.CourseId == courseId
                      && lesson.IsPublished
                select lesson;

            int totalLessons =
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

            int completedLessons =
                await completedQuery.CountAsync();

            decimal progress =
                (decimal)completedLessons
                / totalLessons
                * 100;

            return Math.Round(progress, 2);
        }

        // ==========================================
        // UPDATE PROGRESS
        // ==========================================

        public async Task<bool> UpdateProgressAsync(
            string userId,
            int courseId)
        {
            var userCourse =
                await GetAsync(
                    userId,
                    courseId);

            if (userCourse == null)
            {
                return false;
            }

            decimal progress =
                await GetProgressAsync(
                    userId,
                    courseId);

            userCourse.Progress = progress;

            if (progress >= 100)
            {
                var firstComplete = userCourse.CompletedAt == null;
                userCourse.Progress = 100;
                userCourse.CompletedAt ??= DateTime.UtcNow;
                userCourse.CertificateCode ??= GenerateCertificateCode();

                if (firstComplete)
                {
                    await PromoteLevelAsync(userId, courseId);
                }
            }
            else
            {
                userCourse.CompletedAt = null;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Mã chứng chỉ dạng ALE-XXXXXXXX, dùng để tra cứu công khai.
        /// </summary>
        private static string GenerateCertificateCode()
        {
            var raw = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

            return $"ALE-{raw}";
        }

        // ==========================================
        // COMPLETE COURSE
        // ==========================================

        public async Task<bool> CompleteCourseAsync(
            string userId,
            int courseId)
        {
            var userCourse =
                await GetAsync(
                    userId,
                    courseId);

            if (userCourse == null)
            {
                return false;
            }

            decimal progress =
                await GetProgressAsync(
                    userId,
                    courseId);

            if (progress < 100)
            {
                return false;
            }

            var firstComplete = userCourse.CompletedAt == null;
            userCourse.Progress = 100;

            userCourse.CompletedAt ??= DateTime.UtcNow;

            userCourse.CertificateCode ??= GenerateCertificateCode();

            if (firstComplete)
            {
                await PromoteLevelAsync(userId, courseId);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        private async Task PromoteLevelAsync(string userId, int courseId)
        {
            var course = await _context.courses.FindAsync(courseId);
            var user = await _context.applicationUsers.FindAsync(userId);

            if (course == null || user == null)
            {
                return;
            }

            var courseRank = EnglishLevelScale.Rank(course.Level);
            var userRank = EnglishLevelScale.Rank(user.EnglishLevel);

            if (courseRank == 0)
            {
                return;
            }

            if (userRank == 0 || userRank <= courseRank)
            {
                var next = EnglishLevelScale.Next(course.Level);
                if (!string.IsNullOrWhiteSpace(next))
                {
                    user.EnglishLevel = next;
                }
            }
        }

        public async Task<UserCourse?> GetByCertificateCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var normalized = code.Trim().ToUpperInvariant();

            return await _context.UserCourses
                .Include("Course")
                .Include("ApplicationUser")
                .FirstOrDefaultAsync(x => x.CertificateCode == normalized);
        }

        // ==========================================
        // CONTINUE LESSON
        // ==========================================

        public async Task<Lesson?>
            GetContinueLessonAsync(
                string userId,
                int courseId)
        {
            var query =
                from lesson in _context.Lessons
                join userLesson in _context.UserLessons
                    on lesson.Id equals userLesson.LessonId
                    into lessonProgress
                from userLesson in lessonProgress
                    .Where(x => x.ApplicationUserId == userId)
                    .DefaultIfEmpty()
                where lesson.CourseId == courseId
                      && lesson.IsPublished
                orderby lesson.LessonOrder
                select new
                {
                    Lesson = lesson,
                    Progress = userLesson == null
                        ? 0
                        : userLesson.Progress,
                    IsCompleted = userLesson != null
                        && userLesson.IsCompleted
                };

            var lessons =
                await query.ToListAsync();

            foreach (var item in lessons)
            {
                if (!item.IsCompleted)
                {
                    return item.Lesson;
                }
            }

            return null;
        }

        // ==========================================
        // TOTAL COURSES
        // ==========================================

        public async Task<int> GetTotalCoursesAsync(
            string userId)
        {
            var query =
                from item in _context.UserCourses
                where item.ApplicationUserId == userId
                select item;

            return await query.CountAsync();
        }

        // ==========================================
        // COMPLETED COUNT
        // ==========================================

        public async Task<int>
            GetCompletedCoursesCountAsync(
                string userId)
        {
            var query =
                from item in _context.UserCourses
                where item.ApplicationUserId == userId
                      && item.Progress >= 100
                select item;

            return await query.CountAsync();
        }

        // ==========================================
        // IN PROGRESS COUNT
        // ==========================================

        public async Task<int>
            GetInProgressCoursesCountAsync(
                string userId)
        {
            var query =
                from item in _context.UserCourses
                where item.ApplicationUserId == userId
                      && item.Progress < 100
                select item;

            return await query.CountAsync();
        }
    }
}