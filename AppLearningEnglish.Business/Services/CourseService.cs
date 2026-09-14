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
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;

        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllCourseAsync()
        {
            var query =
                from course in _context.courses
                orderby course.CreatedAt descending
                select course;

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Course>> SearchCourseAsync(
            string? search,
            string? level,
            bool? isPublished)
        {
            IQueryable<Course> query = _context.courses;

            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();

                query =
                    from course in query
                    where course.Name.ToLower().Contains(keyword)
                       || (course.Description != null
                           && course.Description.ToLower().Contains(keyword))
                    select course;
            }

            if (!string.IsNullOrWhiteSpace(level))
            {
                query =
                    from course in query
                    where course.Level == level
                    select course;
            }

            if (isPublished.HasValue)
            {
                bool status = isPublished.Value;

                query =
                    from course in query
                    where course.IsPublished == status
                    select course;
            }

            query =
                from course in query
                orderby course.CreatedAt descending
                select course;

            return await query.ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            var query =
                from course in _context.courses
                where course.Id == id
                select course;

            return await query.FirstOrDefaultAsync();
        }

        public async Task CreateCourseAsync(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;

            _context.courses.Add(course);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCourseAsync(Course course)
        {
            var existingCourse =
                await GetCourseByIdAsync(course.Id);

            if (existingCourse == null)
                return;

            existingCourse.Name = course.Name;
            existingCourse.Description = course.Description;
            existingCourse.ThumbnailUrl = course.ThumbnailUrl;
            existingCourse.Level = course.Level;
            existingCourse.IsPublished = course.IsPublished;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await GetCourseByIdAsync(id);

            if (course == null)
            return false;

            _context.courses.Remove(course);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsNameExistsAsync(
            string name,
            int? id = null)
        {
            string normalizedName =
                name.Trim().ToLower();

            var query =
                from course in _context.courses
                where course.Name.ToLower() == normalizedName
                select course;

            if (id.HasValue)
            {
                query =
                    from course in query
                    where course.Id != id.Value
                    select course;
            }

            return await query.AnyAsync();
        }

        public async Task PublishCourseAsync(int id)
        {
            var course =
                await GetCourseByIdAsync(id);

            if (course == null)
                return;

            course.IsPublished = true;

            await _context.SaveChangesAsync();
        }

        public async Task UnpublishCourseAsync(int id)
        {
            var course =
                await GetCourseByIdAsync(id);

            if (course == null)
                return;

            course.IsPublished = false;

            await _context.SaveChangesAsync();
        }
    }
}