using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business.Services.IServices
{
    public interface ILearnerWorkspaceService
    {
        Task<TodayDashboardViewModel> GetTodayAsync(string userId);

        Task<SkillReportViewModel> GetSkillsAsync(string userId);

        Task<IReadOnlyList<UserNote>> GetNotesAsync(string userId);

        Task<UserNote?> GetLessonNoteAsync(string userId, int lessonId);

        Task SaveLessonNoteAsync(string userId, int lessonId, string body);

        Task DeleteNoteAsync(string userId, int noteId);

        Task<bool> IsBookmarkedAsync(string userId, int lessonId);

        Task ToggleBookmarkAsync(string userId, int lessonId);

        Task<IReadOnlyList<UserBookmark>> GetBookmarksAsync(string userId);

        Task<IReadOnlyList<ProgressExportRow>> GetExportRowsAsync(string userId);
    }

    public class ProgressExportRow
    {
        public string Course { get; set; } = string.Empty;

        public string Lesson { get; set; } = string.Empty;

        public decimal Progress { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Vocab { get; set; } = string.Empty;

        public string Listening { get; set; } = string.Empty;

        public string Exercise { get; set; } = string.Empty;
    }
}
