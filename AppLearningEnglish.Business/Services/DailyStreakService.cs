using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Business.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Services
{
    public class DailyStreakService
        : IDailyStreakService
    {
        private readonly ApplicationDbContext _context;

        public DailyStreakService(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // GET TODAY
        // =========================================

        public async Task<DailyStreak?> GetTodayAsync(
            string userId)
        {
            DateOnly today =
                DateOnly.FromDateTime(
                    DateTime.Now);

            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                      && item.StudyDate == today
                select item;

            return await query
                .FirstOrDefaultAsync();
        }


        // =========================================
        // RECORD STUDY
        // =========================================

        public async Task RecordStudyAsync(
            string userId,
            int minutes,
            int points)
        {
            if (minutes < 0)
            {
                minutes = 0;
            }

            if (points < 0)
            {
                points = 0;
            }

            DateOnly today =
                DateOnly.FromDateTime(
                    DateTime.Now);

            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                      && item.StudyDate == today
                select item;

            var dailyStreak =
                await query.FirstOrDefaultAsync();

            if (dailyStreak == null)
            {
                dailyStreak =
                    new DailyStreak
                    {
                        ApplicationUserId = userId,
                        StudyDate = today,
                        MinutesStudied = minutes,
                        PointsEarned = points
                    };

                _context.DailyStreaks.Add(
                    dailyStreak);
            }
            else
            {
                dailyStreak.MinutesStudied +=
                    minutes;

                dailyStreak.PointsEarned +=
                    points;

                _context.DailyStreaks.Update(
                    dailyStreak);
            }

            await _context.SaveChangesAsync();
        }


        // =========================================
        // CURRENT STREAK
        // =========================================

        public async Task<int>
            GetCurrentStreakAsync(
                string userId)
        {
            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                orderby item.StudyDate descending
                select item;

            var records =
                await query.ToListAsync();

            if (records.Count == 0)
            {
                return 0;
            }

            DateOnly today =
                DateOnly.FromDateTime(
                    DateTime.Now);

            DateOnly latestDate =
                records[0].StudyDate;

            // Nếu hôm nay chưa học,
            // cho phép streak tính từ hôm qua.
            if (latestDate == today)
            {
                // tiếp tục
            }
            else if (latestDate ==
                     today.AddDays(-1))
            {
                // tiếp tục
            }
            else
            {
                return 0;
            }

            int streak = 1;

            for (int i = 1;
                 i < records.Count;
                 i++)
            {
                DateOnly expectedDate =
                    records[i - 1]
                        .StudyDate
                        .AddDays(-1);

                if (records[i].StudyDate ==
                    expectedDate)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }


        // =========================================
        // LONGEST STREAK
        // =========================================

        public async Task<int>
            GetLongestStreakAsync(
                string userId)
        {
            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                orderby item.StudyDate
                select item;

            var records =
                await query.ToListAsync();

            if (records.Count == 0)
            {
                return 0;
            }

            int longest = 1;
            int current = 1;

            for (int i = 1;
                 i < records.Count;
                 i++)
            {
                DateOnly previous =
                    records[i - 1].StudyDate;

                DateOnly currentDate =
                    records[i].StudyDate;

                if (currentDate ==
                    previous.AddDays(1))
                {
                    current++;

                    if (current > longest)
                    {
                        longest = current;
                    }
                }
                else if (currentDate ==
                         previous)
                {
                    // Không tăng streak
                }
                else
                {
                    current = 1;
                }
            }

            return longest;
        }


        // =========================================
        // TOTAL STUDY DAYS
        // =========================================

        public async Task<int>
            GetTotalStudyDaysAsync(
                string userId)
        {
            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                select item;

            return await query.CountAsync();
        }


        // =========================================
        // TOTAL MINUTES
        // =========================================

        public async Task<int>
            GetTotalStudyMinutesAsync(
                string userId)
        {
            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                select item.MinutesStudied;

            return await query.SumAsync();
        }


        // =========================================
        // TOTAL POINTS
        // =========================================

        public async Task<int>
            GetTotalPointsAsync(
                string userId)
        {
            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                select item.PointsEarned;

            return await query.SumAsync();
        }


        // =========================================
        // HISTORY
        // =========================================

        public async Task<IEnumerable<DailyStreak>>
            GetHistoryAsync(
                string userId)
        {
            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                orderby item.StudyDate descending
                select item;

            return await query.ToListAsync();
        }


        // =========================================
        // HISTORY BY DATE
        // =========================================

        public async Task<IEnumerable<DailyStreak>>
            GetHistoryAsync(
                string userId,
                DateOnly fromDate,
                DateOnly toDate)
        {
            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                      && item.StudyDate >= fromDate
                      && item.StudyDate <= toDate
                orderby item.StudyDate descending
                select item;

            return await query.ToListAsync();
        }


        // =========================================
        // MONTH
        // =========================================

        public async Task<IEnumerable<DailyStreak>>
            GetMonthAsync(
                string userId,
                int year,
                int month)
        {
            var query =
                from item in _context.DailyStreaks
                where item.ApplicationUserId == userId
                      && item.StudyDate.Year == year
                      && item.StudyDate.Month == month
                orderby item.StudyDate
                select item;

            return await query.ToListAsync();
        }
    }
}