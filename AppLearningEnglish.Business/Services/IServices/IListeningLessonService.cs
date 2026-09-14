using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business.IService
{
    public interface IListeningLessonService
    {
        Task<IEnumerable<ListeningLesson>>
            GetAllListeningLessonAsync();

        Task<IEnumerable<ListeningLesson>>
            SearchListeningLessonAsync(
                string? search,
                int? lessonId);

        Task<ListeningLesson?>
            GetListeningLessonByIdAsync(
                int id);

        Task<ListeningLesson?>
            GetByLessonIdAsync(
                int lessonId);

        Task CreateListeningLessonAsync(
            ListeningLesson listeningLesson);

        Task UpdateListeningLessonAsync(
            ListeningLesson listeningLesson);

        Task<bool>
            DeleteListeningLessonAsync(
                int id);

        Task<bool>
            IsLessonExistsAsync(
                int lessonId,
                int? id = null);
    }
}