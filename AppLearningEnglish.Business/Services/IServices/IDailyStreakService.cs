using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IDailyStreakService
    {
        Task<DailyStreak?> GetTodayAsync(
            string userId);

        Task RecordStudyAsync(
            string userId,
            int minutes,
            int points);

        Task<int> GetCurrentStreakAsync(
            string userId);

        Task<int> GetLongestStreakAsync(
            string userId);

        Task<int> GetTotalStudyDaysAsync(
            string userId);

        Task<int> GetTotalStudyMinutesAsync(
            string userId);

        Task<int> GetTotalPointsAsync(
            string userId);

        Task<IEnumerable<DailyStreak>>
            GetHistoryAsync(
                string userId);

        Task<IEnumerable<DailyStreak>>
            GetHistoryAsync(
                string userId,
                DateOnly fromDate,
                DateOnly toDate);

        Task<IEnumerable<DailyStreak>>
            GetMonthAsync(
                string userId,
                int year,
                int month);

        
    }
}