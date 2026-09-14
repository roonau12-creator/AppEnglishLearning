using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Services
{
     public class AchievementService : IAchievementService
    {
        private readonly ApplicationDbContext _context;

        public AchievementService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET ALL ACHIEVEMENTS
        // =========================================

        public async Task<IEnumerable<Achievement>>
            GetAllAsync()
        {
            var query =
                from achievement in _context.Achievements
                orderby achievement.RequiredPoints
                select achievement;

            return await query.ToListAsync();
        }

        // =========================================
        // GET BY ID
        // =========================================

        public async Task<Achievement?>
            GetByIdAsync(int id)
        {
            var query =
                from achievement in _context.Achievements
                where achievement.Id == id
                select achievement;

            return await query.FirstOrDefaultAsync();
        }

        // =========================================
        // CREATE
        // =========================================

        public async Task CreateAsync(
            Achievement achievement)
        {
            achievement.Name =
                achievement.Name.Trim();

            _context.Achievements.Add(
                achievement);

            await _context.SaveChangesAsync();
        }

        // =========================================
        // UPDATE
        // =========================================

        public async Task UpdateAsync(
            Achievement achievement)
        {
            var existing =
                await GetByIdAsync(
                    achievement.Id);

            if (existing == null)
            {
                return;
            }

            existing.Name =
                achievement.Name.Trim();

            existing.Description =
                achievement.Description.Trim();

            existing.IconUrl =
                achievement.IconUrl;

            existing.RequiredPoints =
                achievement.RequiredPoints;

            await _context.SaveChangesAsync();
        }

        // =========================================
        // DELETE
        // =========================================

        public async Task<bool> DeleteAsync(
            int id)
        {
            var achievement =
                await GetByIdAsync(id);

            if (achievement == null)
            {
                return false;
            }

            var userAchievementsQuery =
                from item in _context.UserAchievements
                where item.AchievementId == id
                select item;

            var userAchievements =
                await userAchievementsQuery
                    .ToListAsync();

            if (userAchievements.Count > 0)
            {
                _context.UserAchievements.RemoveRange(
                    userAchievements);
            }

            _context.Achievements.Remove(
                achievement);

            await _context.SaveChangesAsync();

            return true;
        }

        // =========================================
        // CHECK NAME
        // =========================================

        public async Task<bool>
            IsNameExistsAsync(
                string name,
                int? id = null)
        {
            string normalizedName =
                name.Trim().ToLower();

            var query =
                from achievement in _context.Achievements
                where achievement.Name.ToLower()
                      == normalizedName
                select achievement;

            var achievements =
                await query.ToListAsync();

            foreach (var achievement
                in achievements)
            {
                if (!id.HasValue ||
                    achievement.Id != id.Value)
                {
                    return true;
                }
            }

            return false;
        }

        // =========================================
        // GET USER ACHIEVEMENTS
        // =========================================

        public async Task<IEnumerable<UserAchievement>>
            GetUserAchievementsAsync(
                string userId)
        {
            var query =
                from item in _context.UserAchievements
                where item.UserId == userId
                orderby item.UnlockedAt descending
                select item;

            return await query
                .Include("Achievement")
                .ToListAsync();
        }

        // =========================================
        // CHECK USER HAS ACHIEVEMENT
        // =========================================

        public async Task<bool>
            HasAchievementAsync(
                string userId,
                int achievementId)
        {
            var query =
                from item in _context.UserAchievements
                where item.UserId == userId
                      && item.AchievementId ==
                         achievementId
                select item;

            return await query.AnyAsync();
        }

        // =========================================
        // SYNC ACHIEVEMENTS
        // =========================================

        public async Task SyncAchievementsAsync(
            string userId)
        {
            int points =
                await GetUserPointsAsync(userId);

            var achievementsQuery =
                from achievement in _context.Achievements
                where achievement.RequiredPoints <= points
                orderby achievement.RequiredPoints
                select achievement;

            var achievements =
                await achievementsQuery.ToListAsync();

            foreach (var achievement
                in achievements)
            {
                bool exists =
                    await HasAchievementAsync(
                        userId,
                        achievement.Id);

                if (exists)
                {
                    continue;
                }

                var userAchievement =
                    new UserAchievement
                    {
                        UserId = userId,
                        AchievementId =
                            achievement.Id,
                        UnlockedAt =
                            DateTime.UtcNow
                    };

                _context.UserAchievements.Add(
                    userAchievement);
            }

            await _context.SaveChangesAsync();
        }

        // =========================================
        // GET USER POINTS
        // =========================================

        public async Task<int>
            GetUserPointsAsync(
                string userId)
        {
            var query =
                from item in _context.UserPoints
                where item.UserId == userId
                select item;

            var userPoint =
                await query.FirstOrDefaultAsync();

            if (userPoint == null)
            {
                return 0;
            }

            return userPoint.Points;
        }

        // =========================================
        // ADD POINTS
        // =========================================

        public async Task<int>
            AddPointsAsync(
                string userId,
                int points)
        {
            if (points <= 0)
            {
                return await GetUserPointsAsync(
                    userId);
            }

            var query =
                from item in _context.UserPoints
                where item.UserId == userId
                select item;

            var userPoint =
                await query.FirstOrDefaultAsync();

            if (userPoint == null)
            {
                userPoint =
                    new UserPoint
                    {
                        UserId = userId,
                        Points = points,
                        UpdatedAt =
                            DateTime.UtcNow
                    };

                _context.UserPoints.Add(
                    userPoint);
            }
            else
            {
                userPoint.Points += points;

                userPoint.UpdatedAt =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await SyncAchievementsAsync(
                userId);

            return userPoint.Points;
        }

        // =========================================
        // UNLOCKED COUNT
        // =========================================

        public async Task<int>
            GetUnlockedCountAsync(
                string userId)
        {
            var query =
                from item in _context.UserAchievements
                where item.UserId == userId
                select item;

            return await query.CountAsync();
        }

        // =========================================
        // TOTAL ACHIEVEMENTS
        // =========================================

        public async Task<int>
            GetTotalAchievementCountAsync()
        {
            return await _context.Achievements
                .CountAsync();
        }
    }
}