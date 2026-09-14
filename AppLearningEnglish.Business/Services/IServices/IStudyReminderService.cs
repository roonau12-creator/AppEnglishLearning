using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IStudyReminderService
    {
        Task<StudyReminderInboxViewModel> GetInboxAsync(string userId);

        Task<IReadOnlyList<StudyReminderCandidate>> GetDueEmailCandidatesAsync(
            CancellationToken cancellationToken = default);

        Task MarkRemindedAsync(
            IEnumerable<string> userIds,
            DateTime utcNow);

        Task LogAsync(string userId, string kind, string title, string body);

        Task<IReadOnlyList<StudyReminderLog>> GetLogsAsync(string userId);

        Task MarkLogsReadAsync(string userId);

        Task EnsureTodayLogsAsync(string userId);

        Task<int> CountUnreadAsync(string userId);

        Task<ReminderSettingsViewModel> GetSettingsAsync(string userId);

        Task SaveSettingsAsync(string userId, ReminderSettingsViewModel settings);
    }
}
