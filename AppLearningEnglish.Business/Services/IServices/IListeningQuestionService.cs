using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business.IService
{
    public interface IListeningQuestionService
    {
        Task<IEnumerable<ListeningQuestion>>
            GetAllListeningQuestionAsync();

        Task<IEnumerable<ListeningQuestion>>
            SearchListeningQuestionAsync(
                string? search,
                int? listeningLessonId);

        Task<IEnumerable<ListeningQuestion>>
            GetByListeningLessonIdAsync(
                int listeningLessonId);

        Task<ListeningQuestion?>
            GetListeningQuestionByIdAsync(
                int id);

        Task CreateListeningQuestionAsync(
            ListeningQuestion listeningQuestion);

        Task UpdateListeningQuestionAsync(
            ListeningQuestion listeningQuestion);

        Task<bool> DeleteListeningQuestionAsync(
            int id);

        Task<bool> IsQuestionExistsAsync(
            int listeningLessonId,
            string question,
            int? id = null);
    }
}