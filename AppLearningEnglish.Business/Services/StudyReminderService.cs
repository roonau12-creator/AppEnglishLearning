using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Services
{
    public class StudyReminderService : IStudyReminderService
    {
        private static readonly TimeSpan ReminderCooldown = TimeSpan.FromHours(20);

        private readonly ApplicationDbContext _context;
        private readonly IUserVocabularyService _userVocabularyService;
        private readonly IDailyStreakService _dailyStreakService;

        public StudyReminderService(
            ApplicationDbContext context,
            IUserVocabularyService userVocabularyService,
            IDailyStreakService dailyStreakService)
        {
            _context = context;
            _userVocabularyService = userVocabularyService;
            _dailyStreakService = dailyStreakService;
        }

        public async Task<StudyReminderInboxViewModel> GetInboxAsync(string userId)
        {
            var today = await _dailyStreakService.GetTodayAsync(userId);
            var streak = await _dailyStreakService.GetCurrentStreakAsync(userId);

            return new StudyReminderInboxViewModel
            {
                DueWordCount = await _userVocabularyService.GetReviewCountAsync(userId),
                ReviewLimit = await _userVocabularyService.GetVocabDailyTargetAsync(userId),
                CurrentStreak = streak,
                StudiedToday = today != null,
                StreakAtRisk = streak > 0 && today == null
            };
        }

        public async Task<IReadOnlyList<StudyReminderCandidate>> GetDueEmailCandidatesAsync(
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow - ReminderCooldown;
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(DateTime.Now);
            var yesterday = today.AddDays(-1);

            var users = await _context.applicationUsers
                .Where(user =>
                    user.EmailConfirmed &&
                    user.Email != null &&
                    user.ReminderEnabled &&
                    (user.LastStudyReminderAt == null || user.LastStudyReminderAt < cutoff))
                .Select(user => new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.VocabDailyTarget,
                    user.ReminderHour,
                    user.TimeZoneId
                })
                .ToListAsync(cancellationToken);

            if (users.Count == 0)
            {
                return Array.Empty<StudyReminderCandidate>();
            }

            var userIds = users.Select(x => x.Id).ToList();

            var dueCounts = await _context.UserVocabularies
                .Where(item =>
                    userIds.Contains(item.ApplicationUserId) &&
                    item.NextReviewAt != null &&
                    item.NextReviewAt <= now)
                .GroupBy(item => item.ApplicationUserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    Count = group.Count()
                })
                .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

            var studiedToday = (await _context.DailyStreaks
                    .Where(item =>
                        userIds.Contains(item.ApplicationUserId) &&
                        item.StudyDate == today)
                    .Select(item => item.ApplicationUserId)
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            var studiedYesterday = (await _context.DailyStreaks
                    .Where(item =>
                        userIds.Contains(item.ApplicationUserId) &&
                        item.StudyDate == yesterday)
                    .Select(item => item.ApplicationUserId)
                    .ToListAsync(cancellationToken))
                .ToHashSet();

            var candidates = new List<StudyReminderCandidate>();

            foreach (var user in users)
            {
                if (!IsWithinReminderHour(now, user.TimeZoneId, user.ReminderHour))
                {
                    continue;
                }

                dueCounts.TryGetValue(user.Id, out var dueCount);
                var streakAtRisk =
                    studiedYesterday.Contains(user.Id) &&
                    !studiedToday.Contains(user.Id);

                if (dueCount <= 0 && !streakAtRisk)
                {
                    continue;
                }

                candidates.Add(new StudyReminderCandidate
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    DisplayName = string.IsNullOrWhiteSpace(user.FullName)
                        ? user.Email!
                        : user.FullName,
                    DueWordCount = dueCount,
                    ReviewLimit = user.VocabDailyTarget > 0 ? user.VocabDailyTarget : 10,
                    StreakAtRisk = streakAtRisk
                });
            }

            return candidates;
        }

        public async Task MarkRemindedAsync(
            IEnumerable<string> userIds,
            DateTime utcNow)
        {
            var ids = userIds.Distinct().ToList();

            if (ids.Count == 0)
            {
                return;
            }

            var users = await _context.applicationUsers
                .Where(user => ids.Contains(user.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                user.LastStudyReminderAt = utcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task LogAsync(string userId, string kind, string title, string body)
        {
            _context.StudyReminderLogs.Add(new StudyReminderLog
            {
                ApplicationUserId = userId,
                Kind = kind,
                Title = title,
                Body = body,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<StudyReminderLog>> GetLogsAsync(string userId)
        {
            return await _context.StudyReminderLogs
                .AsNoTracking()
                .Where(log => log.ApplicationUserId == userId)
                .OrderByDescending(log => log.CreatedAt)
                .Take(80)
                .ToListAsync();
        }

        public async Task MarkLogsReadAsync(string userId)
        {
            var unread = await _context.StudyReminderLogs
                .Where(log => log.ApplicationUserId == userId && log.ReadAt == null)
                .ToListAsync();

            if (unread.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;

            foreach (var log in unread)
            {
                log.ReadAt = now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task EnsureTodayLogsAsync(string userId)
        {
            var inbox = await GetInboxAsync(userId);

            if (inbox.DueWordCount == 0 && !inbox.StreakAtRisk)
            {
                return;
            }

            var today = DateTime.UtcNow.Date;
            var exists = await _context.StudyReminderLogs
                .AnyAsync(log =>
                    log.ApplicationUserId == userId
                    && log.Kind == "inbox"
                    && log.CreatedAt >= today);

            if (exists)
            {
                return;
            }

            var parts = new List<string>();

            if (inbox.DueWordCount > 0)
            {
                parts.Add($"{inbox.DueWordCount} từ đến hạn ôn");
            }

            if (inbox.StreakAtRisk)
            {
                parts.Add("chuỗi ngày học sắp gãy");
            }

            await LogAsync(
                userId,
                "inbox",
                "Nhắc học hôm nay",
                string.Join(" · ", parts));
        }

        public async Task<int> CountUnreadAsync(string userId)
        {
            return await _context.StudyReminderLogs
                .CountAsync(log => log.ApplicationUserId == userId && log.ReadAt == null);
        }

        public async Task<ReminderSettingsViewModel> GetSettingsAsync(string userId)
        {
            var user = await _context.applicationUsers.FirstOrDefaultAsync(x => x.Id == userId);

            return new ReminderSettingsViewModel
            {
                ReminderEnabled = user?.ReminderEnabled ?? true,
                ReminderHour = user?.ReminderHour ?? 19,
                TimeZoneId = string.IsNullOrWhiteSpace(user?.TimeZoneId)
                    ? "Asia/Ho_Chi_Minh"
                    : user.TimeZoneId
            };
        }

        public async Task SaveSettingsAsync(string userId, ReminderSettingsViewModel settings)
        {
            var user = await _context.applicationUsers.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return;
            }

            user.ReminderEnabled = settings.ReminderEnabled;
            user.ReminderHour = Math.Clamp(settings.ReminderHour, 0, 23);
            user.TimeZoneId = string.IsNullOrWhiteSpace(settings.TimeZoneId)
                ? "Asia/Ho_Chi_Minh"
                : settings.TimeZoneId.Trim();

            await _context.SaveChangesAsync();
        }

        internal static bool IsWithinReminderHour(DateTime utcNow, string? timeZoneId, int hour)
        {
            try
            {
                var zone = ResolveTimeZone(timeZoneId);
                var local = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(utcNow, DateTimeKind.Utc),
                    zone);
                return local.Hour == Math.Clamp(hour, 0, 23);
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.Now.Hour == Math.Clamp(hour, 0, 23);
            }
        }

        private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(timeZoneId))
            {
                timeZoneId = "Asia/Ho_Chi_Minh";
            }

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                return timeZoneId switch
                {
                    "Asia/Ho_Chi_Minh" or "Asia/Bangkok" =>
                        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"),
                    "UTC" => TimeZoneInfo.Utc,
                    _ => TimeZoneInfo.Local
                };
            }
        }
    }
}
