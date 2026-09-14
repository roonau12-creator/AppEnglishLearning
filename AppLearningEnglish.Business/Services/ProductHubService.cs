using System.Globalization;
using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Services
{
    public class ProductHubService : IProductHubService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDailyStreakService _streaks;
        private readonly IUserVocabularyService _vocab;

        public ProductHubService(
            ApplicationDbContext context,
            IDailyStreakService streaks,
            IUserVocabularyService vocab)
        {
            _context = context;
            _streaks = streaks;
            _vocab = vocab;
        }

        public async Task AwardStudyAsync(string userId, int points, int minutes)
        {
            if (string.IsNullOrWhiteSpace(userId) || (points <= 0 && minutes <= 0))
            {
                return;
            }

            var user = await _context.applicationUsers.FindAsync(userId);
            if (user == null)
            {
                return;
            }

            var coins = Math.Max(0, points);
            var xp = Math.Max(0, points * 2 + minutes);
            user.Coins += coins;
            user.PlayerXp += xp;
            user.PlayerLevel = 1 + user.PlayerXp / 100;

            await BumpChallengeAsync(userId, "daily-minutes", minutes);
            await BumpChallengeAsync(userId, "daily-xp", xp);
            await BumpChallengeAsync(userId, "weekly-xp", xp);
            await BumpChallengeAsync(userId, "monthly-minutes", minutes);

            await _context.SaveChangesAsync();
        }

        public async Task NotifyAsync(string userId, string kind, string title, string? body, string? link)
        {
            var exists = await _context.InAppNotifications.AnyAsync(x =>
                x.ApplicationUserId == userId
                && x.Kind == kind
                && x.CreatedAt >= DateTime.UtcNow.Date);

            if (exists)
            {
                return;
            }

            _context.InAppNotifications.Add(new InAppNotification
            {
                ApplicationUserId = userId,
                Kind = kind,
                Title = title,
                Body = body,
                Link = link,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public Task<int> CountUnreadAsync(string userId) =>
            _context.InAppNotifications.CountAsync(x =>
                x.ApplicationUserId == userId && !x.IsRead);

        public async Task<IReadOnlyList<InAppNotification>> GetInboxAsync(string userId, int take = 30)
        {
            return await _context.InAppNotifications
                .AsNoTracking()
                .Where(x => x.ApplicationUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task MarkReadAsync(string userId, int? id = null)
        {
            var items = await _context.InAppNotifications
                .Where(x => x.ApplicationUserId == userId && !x.IsRead
                    && (id == null || x.Id == id))
                .ToListAsync();

            foreach (var item in items)
            {
                item.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task EnsureDailyAsync(string userId)
        {
            var inbox = await _streaks.GetTodayAsync(userId);
            var streak = await _streaks.GetCurrentStreakAsync(userId);
            var due = await _vocab.GetReviewCountAsync(userId);

            if (streak > 0 && inbox == null)
            {
                await NotifyAsync(
                    userId,
                    "streak",
                    "Chuỗi ngày sắp gãy",
                    $"Bạn đang có {streak} ngày. Học vài phút để giữ streak.",
                    "/chuoi-ngay");
            }

            if (due > 0)
            {
                await NotifyAsync(
                    userId,
                    "vocab",
                    "Từ mới / từ đến hạn",
                    $"{due} từ đang chờ ôn.",
                    "/tu-vung/Quiz");
            }

            await NotifyAsync(
                userId,
                "quiz",
                "Quiz sẵn sàng",
                "Làm một bài quiz ngắn hoặc xem lại câu sai.",
                "/quiz");

            await EnsureChallengeRowsAsync(userId);
        }

        public async Task RecordMistakeAsync(
            string userId,
            string kind,
            string prompt,
            string expected,
            string? userAnswer)
        {
            _context.QuizMistakes.Add(new QuizMistake
            {
                ApplicationUserId = userId,
                Kind = kind,
                Prompt = prompt,
                Expected = expected,
                UserAnswer = userAnswer,
                CreatedAt = DateTime.UtcNow
            });

            await BumpChallengeAsync(userId, "daily-quiz", 1);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<QuizMistake>> GetOpenMistakesAsync(string userId)
        {
            return await _context.QuizMistakes
                .AsNoTracking()
                .Where(x => x.ApplicationUserId == userId && !x.IsReviewed)
                .OrderByDescending(x => x.CreatedAt)
                .Take(40)
                .ToListAsync();
        }

        public async Task MarkMistakeReviewedAsync(string userId, int id)
        {
            var item = await _context.QuizMistakes
                .FirstOrDefaultAsync(x => x.Id == id && x.ApplicationUserId == userId);

            if (item == null)
            {
                return;
            }

            item.IsReviewed = true;
            await _context.SaveChangesAsync();
        }

        public async Task SaveGrammarProgressAsync(string userId, int topicId, int percent)
        {
            var row = await _context.GrammarProgresses
                .FirstOrDefaultAsync(x => x.ApplicationUserId == userId && x.GrammarTopicId == topicId);

            if (row == null)
            {
                row = new GrammarProgress
                {
                    ApplicationUserId = userId,
                    GrammarTopicId = topicId
                };
                _context.GrammarProgresses.Add(row);
            }

            row.LastPercent = percent;
            row.BestPercent = Math.Max(row.BestPercent, percent);
            row.Attempts += 1;
            row.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyDictionary<int, GrammarProgress>> GetGrammarProgressAsync(string userId)
        {
            var rows = await _context.GrammarProgresses
                .AsNoTracking()
                .Where(x => x.ApplicationUserId == userId)
                .ToListAsync();

            return rows.ToDictionary(x => x.GrammarTopicId);
        }

        public async Task<IReadOnlyList<UserChallenge>> GetActiveChallengesAsync(string userId)
        {
            await EnsureChallengeRowsAsync(userId);

            var daily = PeriodKey("daily");
            var weekly = PeriodKey("weekly");
            var monthly = PeriodKey("monthly");

            return await _context.UserChallenges
                .AsNoTracking()
                .Include(x => x.Challenge)
                .Where(x => x.ApplicationUserId == userId
                    && (x.PeriodKey == daily || x.PeriodKey == weekly || x.PeriodKey == monthly))
                .OrderBy(x => x.Challenge!.Period)
                .ThenBy(x => x.ChallengeId)
                .ToListAsync();
        }

        public async Task<PlayerStatsViewModel> GetPlayerAsync(string userId)
        {
            var user = await _context.applicationUsers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            var xp = user?.PlayerXp ?? 0;
            return new PlayerStatsViewModel
            {
                Coins = user?.Coins ?? 0,
                PlayerXp = xp,
                PlayerLevel = user?.PlayerLevel > 0 ? user.PlayerLevel : 1 + xp / 100,
                XpIntoLevel = xp % 100,
                XpForNext = 100
            };
        }

        private async Task EnsureChallengeRowsAsync(string userId)
        {
            var challenges = await _context.Challenges.AsNoTracking().ToListAsync();
            if (challenges.Count == 0)
            {
                return;
            }

            foreach (var challenge in challenges)
            {
                var key = PeriodKey(challenge.Period);
                var exists = await _context.UserChallenges.AnyAsync(x =>
                    x.ApplicationUserId == userId
                    && x.ChallengeId == challenge.Id
                    && x.PeriodKey == key);

                if (!exists)
                {
                    _context.UserChallenges.Add(new UserChallenge
                    {
                        ApplicationUserId = userId,
                        ChallengeId = challenge.Id,
                        PeriodKey = key
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task BumpChallengeAsync(string userId, string code, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            var challenge = await _context.Challenges.FirstOrDefaultAsync(x => x.Code == code);
            if (challenge == null)
            {
                return;
            }

            var key = PeriodKey(challenge.Period);
            var row = await _context.UserChallenges.FirstOrDefaultAsync(x =>
                x.ApplicationUserId == userId
                && x.ChallengeId == challenge.Id
                && x.PeriodKey == key);

            if (row == null)
            {
                row = new UserChallenge
                {
                    ApplicationUserId = userId,
                    ChallengeId = challenge.Id,
                    PeriodKey = key
                };
                _context.UserChallenges.Add(row);
            }

            if (row.IsCompleted)
            {
                return;
            }

            row.Progress += amount;
            if (row.Progress >= challenge.Target)
            {
                row.IsCompleted = true;
                row.CompletedAt = DateTime.UtcNow;
                var user = await _context.applicationUsers.FindAsync(userId);
                if (user != null)
                {
                    user.Coins += challenge.RewardCoins;
                    user.PlayerXp += challenge.RewardXp;
                    user.PlayerLevel = 1 + user.PlayerXp / 100;
                }

                await NotifyAsync(
                    userId,
                    "achievement",
                    "Hoàn thành thử thách",
                    $"{challenge.Title} · +{challenge.RewardCoins} xu · +{challenge.RewardXp} XP",
                    "/thu-thach");
            }
        }

        private static string PeriodKey(string period)
        {
            var today = DateTime.UtcNow.Date;
            return period switch
            {
                "weekly" => $"{ISOWeek.GetYear(today)}-W{ISOWeek.GetWeekOfYear(today):00}",
                "monthly" => today.ToString("yyyy-MM"),
                _ => today.ToString("yyyy-MM-dd")
            };
        }
    }
}
