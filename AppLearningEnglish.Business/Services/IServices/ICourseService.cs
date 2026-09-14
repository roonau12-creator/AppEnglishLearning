using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;
namespace AppLearningEnglish.Business.Services.IServices
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllCourseAsync();

        Task<IEnumerable<Course>> SearchCourseAsync(
            string? search,
            string? level,
            bool? isPublished);

        Task<Course?> GetCourseByIdAsync(int id);

        Task CreateCourseAsync(Course course);

        Task UpdateCourseAsync(Course course);

        Task<bool> DeleteCourseAsync(int id);

        Task<bool> IsNameExistsAsync(
            string name,
            int? id = null);

        Task PublishCourseAsync(int id);

        Task UnpublishCourseAsync(int id);
    }
}