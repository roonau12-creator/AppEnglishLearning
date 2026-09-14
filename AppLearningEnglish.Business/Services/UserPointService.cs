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
    public class UserPointService : IUserPointService
    {
        private readonly ApplicationDbContext _context;

        public UserPointService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET USER POINT
        // ==========================================

        public async Task<UserPoint?> GetAsync(
            string userId)
        {
            var query =
                from item in _context.UserPoints
                where item.UserId == userId
                select item;

            return await query.FirstOrDefaultAsync();
        }

        // ==========================================
        // GET POINTS
        // ==========================================

        public async Task<int> GetPointsAsync(
            string userId)
        {
            var userPoint =
                await GetAsync(userId);

            if (userPoint == null)
            {
                return 0;
            }

            return userPoint.Points;
        }

        // ==========================================
        // CREATE USER POINT
        // ==========================================

        public async Task<UserPoint> CreateAsync(
            string userId)
        {
            var existing =
                await GetAsync(userId);

            if (existing != null)
            {
                return existing;
            }

            var userPoint =
                new UserPoint
                {
                    UserId = userId,
                    Points = 0,
                    UpdatedAt =
                        DateTime.UtcNow
                };

            _context.UserPoints.Add(
                userPoint);

            await _context.SaveChangesAsync();

            return userPoint;
        }

        // ==========================================
        // ADD POINTS
        // ==========================================

        public async Task<int> AddPointsAsync(
            string userId,
            int points)
        {
            if (points <= 0)
            {
                return await GetPointsAsync(
                    userId);
            }

            var userPoint =
                await GetAsync(userId);

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

            return userPoint.Points;
        }

        // ==========================================
        // REMOVE POINTS
        // ==========================================

        public async Task<int> RemovePointsAsync(
            string userId,
            int points)
        {
            if (points <= 0)
            {
                return await GetPointsAsync(
                    userId);
            }

            var userPoint =
                await GetAsync(userId);

            if (userPoint == null)
            {
                return 0;
            }

            userPoint.Points -= points;

            if (userPoint.Points < 0)
            {
                userPoint.Points = 0;
            }

            userPoint.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return userPoint.Points;
        }

        // ==========================================
        // SET POINTS
        // ==========================================

        public async Task<int> SetPointsAsync(
            string userId,
            int points)
        {
            if (points < 0)
            {
                points = 0;
            }

            var userPoint =
                await GetAsync(userId);

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
                userPoint.Points = points;

                userPoint.UpdatedAt =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return userPoint.Points;
        }

        // ==========================================
        // CHECK ENOUGH POINTS
        // ==========================================

        public async Task<bool>
            HasEnoughPointsAsync(
                string userId,
                int requiredPoints)
        {
            int points =
                await GetPointsAsync(userId);

            return points >= requiredPoints;
        }

        // ==========================================
        // LEADERBOARD
        // ==========================================

        public async Task<IEnumerable<UserPoint>>
            GetLeaderboardAsync(
                int limit = 10)
        {
            if (limit <= 0)
            {
                limit = 10;
            }

            if (limit > 100)
            {
                limit = 100;
            }

            var query =
                from item in _context.UserPoints
                orderby item.Points descending
                select item;

            return await query
                .Include("ApplicationUser")
                .Take(limit)
                .ToListAsync();
        }
    }
}