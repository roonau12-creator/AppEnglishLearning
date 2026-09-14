using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IProductHubService
    {
        Task AwardStudyAsync(string userId, int points, int minutes);

        Task NotifyAsync(string userId, string kind, string title, string? body, string? link);

        Task<int> CountUnreadAsync(string userId);

        Task<IReadOnlyList<InAppNotification>> GetInboxAsync(string userId, int take = 30);

        Task MarkReadAsync(string userId, int? id = null);

        Task EnsureDailyAsync(string userId);

        Task RecordMistakeAsync(string userId, string kind, string prompt, string expected, string? userAnswer);

        Task<IReadOnlyList<QuizMistake>> GetOpenMistakesAsync(string userId);

        Task MarkMistakeReviewedAsync(string userId, int id);

        Task SaveGrammarProgressAsync(string userId, int topicId, int percent);

        Task<IReadOnlyDictionary<int, GrammarProgress>> GetGrammarProgressAsync(string userId);

        Task<IReadOnlyList<UserChallenge>> GetActiveChallengesAsync(string userId);

        Task<PlayerStatsViewModel> GetPlayerAsync(string userId);
    }
}
