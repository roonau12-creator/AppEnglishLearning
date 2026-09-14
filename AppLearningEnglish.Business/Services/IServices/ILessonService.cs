using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business.Services.IServices
{
     public interface ILessonService
    {
        Task<IEnumerable<Lesson>> GetAllLessonAsync();

        Task<IEnumerable<Lesson>> SearchLessonAsync(
            string? search,
            int? courseId,
            int? topicId,
            bool? isPublished);

        Task<IEnumerable<Lesson>> GetPublishedByCourseIdAsync(int courseId);

        Task<Lesson?> GetLessonByIdAsync(int id);

        Task CreateLessonAsync(Lesson lesson);

        Task UpdateLessonAsync(Lesson lesson);

        Task<bool> DeleteLessonAsync(int id);

        Task<bool> IsTitleExistsAsync(
            string title,
            int courseId,
            int? id = null);

        Task PublishLessonAsync(int id);

        Task UnpublishLessonAsync(int id);
    }
}