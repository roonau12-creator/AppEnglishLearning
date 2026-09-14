using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IUserPointService
    {
        Task<UserPoint?> GetAsync(string userId);

        Task<int> GetPointsAsync(string userId);

        Task<UserPoint> CreateAsync(string userId);

        Task<int> AddPointsAsync(
            string userId,
            int points);

        Task<int> RemovePointsAsync(
            string userId,
            int points);

        Task<int> SetPointsAsync(
            string userId,
            int points);

        Task<bool> HasEnoughPointsAsync(
            string userId,
            int requiredPoints);

        Task<IEnumerable<UserPoint>>
            GetLeaderboardAsync(int limit = 10);
    }
}