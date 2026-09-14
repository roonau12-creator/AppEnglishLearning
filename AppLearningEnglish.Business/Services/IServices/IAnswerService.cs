using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business.IService
{
    public interface IAnswerService
    {
        Task<IEnumerable<Answer>> GetAllAnswerAsync();

        Task<IEnumerable<Answer>> SearchAnswerAsync(
            string? search,
            int? questionId);

        Task<IEnumerable<Answer>> GetByQuestionIdAsync(
            int questionId);

        Task<Answer?> GetAnswerByIdAsync(
            int id);

        Task CreateAnswerAsync(
            Answer answer);

        Task UpdateAnswerAsync(
            Answer answer);

        Task<bool> DeleteAnswerAsync(
            int id);

        Task<bool> IsAnswerExistsAsync(
            int questionId,
            string answerText,
            int? id = null);

        Task SetCorrectAnswerAsync(
            int id);

        Task SetIncorrectAnswerAsync(
            int id);
    }
}