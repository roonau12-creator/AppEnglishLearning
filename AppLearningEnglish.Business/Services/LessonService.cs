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
    public class LessonService : ILessonService
    {
        private readonly ApplicationDbContext _context;

        public LessonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Lesson>> GetAllLessonAsync()
        {
            var query =
                from lesson in _context.Lessons
                orderby lesson.LessonOrder ascending
                select lesson;

            return await query
                .Include("Course")
                .Include("Topic")
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> SearchLessonAsync(
            string? search,
            int? courseId,
            int? topicId,
            bool? isPublished)
        {
            IQueryable<Lesson> query = _context.Lessons;

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword = search.Trim().ToLower();

                query =
                    from lesson in query
                    where lesson.Title.ToLower().Contains(keyword)
                       || (
                            lesson.Description != null
                            && lesson.Description
                                .ToLower()
                                .Contains(keyword)
                          )
                    select lesson;
            }

            // Filter Course
            if (courseId.HasValue && courseId.Value > 0)
            {
                int selectedCourseId = courseId.Value;

                query =
                    from lesson in query
                    where lesson.CourseId == selectedCourseId
                    select lesson;
            }

            // Filter Topic
            if (topicId.HasValue && topicId.Value > 0)
            {
                int selectedTopicId = topicId.Value;

                query =
                    from lesson in query
                    where lesson.TopicId == selectedTopicId
                    select lesson;
            }

            // Filter Published
            if (isPublished.HasValue)
            {
                bool status = isPublished.Value;

                query =
                    from lesson in query
                    where lesson.IsPublished == status
                    select lesson;
            }

            query =
                from lesson in query
                orderby lesson.LessonOrder ascending
                select lesson;

            return await query
                .Include("Course")
                .Include("Topic")
                .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetPublishedByCourseIdAsync(int courseId)
        {
            var query =
                from lesson in _context.Lessons
                where lesson.CourseId == courseId
                      && lesson.IsPublished
                orderby lesson.LessonOrder
                select lesson;

            return await query
                .Include("Topic")
                .ToListAsync();
        }

        public async Task<Lesson?> GetLessonByIdAsync(int id)
        {
            var query =
                from lesson in _context.Lessons
                where lesson.Id == id
                select lesson;

            return await query
                .Include("Course")
                .Include("Topic")
                .Include("ListeningLesson")
                .FirstOrDefaultAsync();
        }

        public async Task CreateLessonAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateLessonAsync(Lesson lesson)
        {
            var existingLesson =
                await GetLessonByIdAsync(lesson.Id);

            if (existingLesson == null)
                return;

            existingLesson.CourseId = lesson.CourseId;
            existingLesson.TopicId = lesson.TopicId;
            existingLesson.Title = lesson.Title;
            existingLesson.Description = lesson.Description;
            existingLesson.Content = lesson.Content;
            existingLesson.VideoUrl = lesson.VideoUrl;
            existingLesson.GrammarNotes = lesson.GrammarNotes;
            existingLesson.LessonOrder = lesson.LessonOrder;
            existingLesson.DurationMinutes =
                lesson.DurationMinutes;
            existingLesson.IsPublished =
                lesson.IsPublished;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteLessonAsync(int id)
        {
            var lesson =
                await GetLessonByIdAsync(id);

            if (lesson == null)
                return false;

            _context.Lessons.Remove(lesson);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsTitleExistsAsync(
            string title,
            int courseId,
            int? id = null)
        {
            string normalizedTitle =
                title.Trim().ToLower();

            var query =
                from lesson in _context.Lessons
                where lesson.CourseId == courseId
                   && lesson.Title.ToLower() == normalizedTitle
                select lesson;

            if (id.HasValue)
            {
                query =
                    from lesson in query
                    where lesson.Id != id.Value
                    select lesson;
            }

            return await query.AnyAsync();
        }

        public async Task PublishLessonAsync(int id)
        {
            var lesson =
                await GetLessonByIdAsync(id);

            if (lesson == null)
                return;

            lesson.IsPublished = true;

            await _context.SaveChangesAsync();
        }

        public async Task UnpublishLessonAsync(int id)
        {
            var lesson =
                await GetLessonByIdAsync(id);

            if (lesson == null)
                return;

            lesson.IsPublished = false;

            await _context.SaveChangesAsync();
        }
    }
}