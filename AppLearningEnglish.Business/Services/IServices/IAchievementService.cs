using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business.Services.IServices
{
     public interface IAchievementService
    {
        // Achievement
        Task<IEnumerable<Achievement>> GetAllAsync();

        Task<Achievement?> GetByIdAsync(int id);

        Task CreateAsync(Achievement achievement);

        Task UpdateAsync(Achievement achievement);

        Task<bool> DeleteAsync(int id);

        Task<bool> IsNameExistsAsync(
            string name,
            int? id = null);

        // User Achievement

        Task<IEnumerable<UserAchievement>>
            GetUserAchievementsAsync(
                string userId);

        Task<bool> HasAchievementAsync(
            string userId,
            int achievementId);

        Task SyncAchievementsAsync(
            string userId);

        // Points
        Task<int> GetUserPointsAsync(
            string userId);

        Task<int> AddPointsAsync(
            string userId,
            int points);

        // Statistics
        Task<int> GetUnlockedCountAsync(
            string userId);

        Task<int> GetTotalAchievementCountAsync();
    }
}