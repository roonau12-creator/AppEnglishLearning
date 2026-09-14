using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Business.Services
{
    public class LearnerWorkspaceService : ILearnerWorkspaceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserVocabularyService _vocab;
        private readonly IDailyStreakService _streaks;
        private readonly IUserLessonService _userLessons;
        private readonly IUserCourseService _userCourses;
        private readonly IStudyReminderService _reminders;
        private readonly IExerciseAttemptService _attempts;
        private readonly IProductHubService _product;
        private readonly IUserPointService _points;

        public LearnerWorkspaceService(
            ApplicationDbContext context,
            IUserVocabularyService vocab,
            IDailyStreakService streaks,
            IUserLessonService userLessons,
            IUserCourseService userCourses,
            IStudyReminderService reminders,
            IExerciseAttemptService attempts,
            IProductHubService product,
            IUserPointService points)
        {
            _context = context;
            _vocab = vocab;
            _streaks = streaks;
            _userLessons = userLessons;
            _userCourses = userCourses;
            _reminders = reminders;
            _attempts = attempts;
            _product = product;
            _points = points;
        }

        public async Task<TodayDashboardViewModel> GetTodayAsync(string userId)
        {
            var user = await _context.applicationUsers.FindAsync(userId);
            var today = await _streaks.GetTodayAsync(userId);
            var inbox = await _reminders.GetInboxAsync(userId);
            await _reminders.EnsureTodayLogsAsync(userId);

            var inProgress = (await _userLessons.GetInProgressLessonsAsync(userId)).ToList();
            var courses = (await _userCourses.GetInProgressCoursesAsync(userId)).Take(4).ToList();
            var weak = await GetWeakListAsync(userId, 5);
            var minutes = today?.MinutesStudied ?? 0;
            var goal = user?.DailyGoalMinutes > 0 ? user.DailyGoalMinutes : 30;
            var goalPercent = Math.Min(100, (int)Math.Round(minutes * 100d / goal));
            await _product.EnsureDailyAsync(userId);

            var attempts = (await _attempts.GetAllAsync(userId)).ToList();
            var wordsLearned = await _vocab.GetTotalWordsAsync(userId);
            var board = (await _points.GetLeaderboardAsync(5)).ToList();
            var player = await _product.GetPlayerAsync(userId);

            return new TodayDashboardViewModel
            {
                DisplayName = string.IsNullOrWhiteSpace(user?.FullName)
                    ? user?.Email ?? "Bạn"
                    : user!.FullName!,
                EnglishLevel = user?.EnglishLevel,
                MinutesToday = minutes,
                DailyGoalMinutes = goal,
                GoalPercent = goalPercent,
                DueWordCount = inbox.DueWordCount,
                ReviewLimit = inbox.ReviewLimit,
                CurrentStreak = inbox.CurrentStreak,
                StreakAtRisk = inbox.StreakAtRisk,
                StudiedToday = inbox.StudiedToday,
                ContinueLesson = inProgress.FirstOrDefault(),
                ActiveCourses = courses,
                WeakWords = weak,
                UnreadReminders = await _reminders.CountUnreadAsync(userId),
                UnreadNotifications = await _product.CountUnreadAsync(userId),
                WordsLearned = wordsLearned,
                VocabGoalTotal = user?.VocabGoalTotal > 0 ? user.VocabGoalTotal : 2000,
                AverageScore = attempts.Count == 0
                    ? 0
                    : Math.Round(attempts.Average(x => (decimal)x.Score), 1),
                Coins = player.Coins,
                PlayerXp = player.PlayerXp,
                PlayerLevel = player.PlayerLevel,
                LearningGoalKind = user?.LearningGoalKind ?? "vocab",
                IeltsBandTarget = user?.IeltsBandTarget,
                Leaderboard = board.Select(item => new LeaderboardRowViewModel
                {
                    Name = string.IsNullOrWhiteSpace(item.ApplicationUser?.FullName)
                        ? item.ApplicationUser?.Email ?? "Học viên"
                        : item.ApplicationUser!.FullName!,
                    Points = item.Points,
                    IsMe = item.UserId == userId
                }).ToList(),
                TodayPlan =
                [
                    new() { Title = "Vocabulary", Controller = "Vocabulary", Action = "Quiz", RouteMode = "type", Icon = "bi-translate" },
                    new() { Title = "Grammar", Controller = "Grammar", Action = "Index", Icon = "bi-spellcheck" },
                    new() { Title = "Listening", Controller = "Listening", Action = "Index", Icon = "bi-headphones" },
                    new() { Title = "Quiz", Controller = "QuizHub", Action = "Index", Icon = "bi-ui-checks" }
                ],
                Challenges = (await _product.GetActiveChallengesAsync(userId)).ToList()
            };
        }

        public async Task<SkillReportViewModel> GetSkillsAsync(string userId)
        {
            var today = await _streaks.GetTodayAsync(userId);
            var attempts = (await _attempts.GetAllAsync(userId)).ToList();
            var lessons = (await _userLessons.GetUserLessonsAsync(userId)).ToList();
            var weak = await GetWeakListAsync(userId, 20);
            var totalWords = await _vocab.GetTotalWordsAsync(userId);
            var mastered = await _vocab.GetMasteredCountAsync(userId);
            var due = await _vocab.GetReviewCountAsync(userId);
            var vocabPercent = totalWords == 0
                ? 0
                : Math.Round(mastered * 100m / totalWords, 1);

            var practices = await _context.PracticeAttempts
                .AsNoTracking()
                .Where(x => x.ApplicationUserId == userId)
                .ToListAsync();
            var grammar = await _product.GetGrammarProgressAsync(userId);
            var started = Math.Max(1, lessons.Count);
            var streaks = await _context.DailyStreaks
                .AsNoTracking()
                .Where(x => x.ApplicationUserId == userId)
                .OrderByDescending(x => x.StudyDate)
                .Take(90)
                .ToListAsync();

            return new SkillReportViewModel
            {
                TotalWords = totalWords,
                MasteredWords = mastered,
                DueWords = due,
                WeakWords = weak.Count,
                VocabPercent = vocabPercent,
                ListeningDone = lessons.Count(x => x.ListeningDone),
                LessonsStarted = lessons.Count,
                ExerciseAverage = attempts.Count == 0
                    ? 0
                    : Math.Round(attempts.Average(x => (decimal)x.Score), 1),
                ExerciseAttempts = attempts.Count,
                MinutesToday = today?.MinutesStudied ?? 0,
                CurrentStreak = await _streaks.GetCurrentStreakAsync(userId),
                WeakList = weak,
                GrammarPercent = grammar.Count == 0
                    ? 0
                    : Math.Round((decimal)grammar.Values.Average(x => x.BestPercent), 1),
                ListeningPercent = Math.Round(lessons.Count(x => x.ListeningDone) * 100m / started, 1),
                SpeakingPercent = AverageKind(practices, "speaking", "conversation"),
                ReadingPercent = Math.Round(lessons.Count(x => x.ReadingDone) * 100m / started, 1),
                WritingPercent = AverageKind(practices, "writing"),
                DailyPoints = streaks
                    .OrderBy(x => x.StudyDate)
                    .TakeLast(14)
                    .Select(x => new ProgressPointViewModel
                    {
                        Label = x.StudyDate.ToString("dd/MM"),
                        Minutes = x.MinutesStudied,
                        Xp = x.PointsEarned
                    })
                    .ToList(),
                WeeklyPoints = GroupStreaks(streaks, x =>
                    $"{x.StudyDate.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(x.StudyDate.ToDateTime(TimeOnly.MinValue)):00}"),
                MonthlyPoints = GroupStreaks(streaks, x => x.StudyDate.ToString("yyyy-MM"))
            };
        }

        private static decimal AverageKind(IEnumerable<PracticeAttempt> items, params string[] kinds)
        {
            var set = items.Where(x => kinds.Contains(x.Kind, StringComparer.OrdinalIgnoreCase)).ToList();
            return set.Count == 0 ? 0 : Math.Round((decimal)set.Average(x => x.Score), 1);
        }

        private static List<ProgressPointViewModel> GroupStreaks(
            IEnumerable<DailyStreak> streaks,
            Func<DailyStreak, string> key)
        {
            return streaks
                .GroupBy(key)
                .OrderBy(g => g.Key)
                .Select(g => new ProgressPointViewModel
                {
                    Label = g.Key,
                    Minutes = g.Sum(x => x.MinutesStudied),
                    Xp = g.Sum(x => x.PointsEarned)
                })
                .ToList();
        }

        public async Task<IReadOnlyList<UserNote>> GetNotesAsync(string userId)
        {
            return await _context.UserNotes
                .AsNoTracking()
                .Include(x => x.Lesson)
                .Where(x => x.ApplicationUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(80)
                .ToListAsync();
        }

        public async Task<UserNote?> GetLessonNoteAsync(string userId, int lessonId)
        {
            return await _context.UserNotes
                .Where(x => x.ApplicationUserId == userId && x.LessonId == lessonId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task SaveLessonNoteAsync(string userId, int lessonId, string body)
        {
            var text = (body ?? string.Empty).Trim();
            var lesson = await _context.Lessons.FindAsync(lessonId);
            var existing = await GetLessonNoteAsync(userId, lessonId);

            if (string.IsNullOrWhiteSpace(text))
            {
                if (existing != null)
                {
                    _context.UserNotes.Remove(existing);
                    await _context.SaveChangesAsync();
                }

                return;
            }

            if (existing == null)
            {
                _context.UserNotes.Add(new UserNote
                {
                    ApplicationUserId = userId,
                    LessonId = lessonId,
                    CourseId = lesson?.CourseId,
                    Body = text,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Body = text;
                existing.CourseId = lesson?.CourseId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(string userId, int noteId)
        {
            var note = await _context.UserNotes
                .FirstOrDefaultAsync(x => x.Id == noteId && x.ApplicationUserId == userId);

            if (note == null)
            {
                return;
            }

            _context.UserNotes.Remove(note);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsBookmarkedAsync(string userId, int lessonId)
        {
            return await _context.UserBookmarks
                .AnyAsync(x => x.ApplicationUserId == userId && x.LessonId == lessonId);
        }

        public async Task ToggleBookmarkAsync(string userId, int lessonId)
        {
            var item = await _context.UserBookmarks
                .FirstOrDefaultAsync(x => x.ApplicationUserId == userId && x.LessonId == lessonId);

            if (item == null)
            {
                _context.UserBookmarks.Add(new UserBookmark
                {
                    ApplicationUserId = userId,
                    LessonId = lessonId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                _context.UserBookmarks.Remove(item);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<UserBookmark>> GetBookmarksAsync(string userId)
        {
            return await _context.UserBookmarks
                .AsNoTracking()
                .Include(x => x.Lesson)
                .ThenInclude(x => x!.Course)
                .Where(x => x.ApplicationUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<ProgressExportRow>> GetExportRowsAsync(string userId)
        {
            var lessons = await _userLessons.GetUserLessonsAsync(userId);

            return lessons
                .Select(item => new ProgressExportRow
                {
                    Course = item.Lesson?.Course?.Name ?? "",
                    Lesson = item.Lesson?.Title ?? $"Bài {item.LessonId}",
                    Progress = item.Progress,
                    Status = item.IsCompleted ? "Hoàn thành" : "Đang học",
                    Vocab = item.VocabDone ? "Xong" : "Chưa",
                    Listening = item.ListeningDone ? "Xong" : "Chưa",
                    Exercise = item.ExerciseDone ? "Xong" : "Chưa"
                })
                .ToList();
        }

        private async Task<List<UserVocabulary>> GetWeakListAsync(string userId, int take)
        {
            return await _context.UserVocabularies
                .AsNoTracking()
                .Include("Word.Meanings")
                .Where(x =>
                    x.ApplicationUserId == userId
                    && (x.WrongCount >= 2 || (x.WrongCount > x.CorrectCount && x.WrongCount > 0)))
                .OrderByDescending(x => x.WrongCount)
                .ThenBy(x => x.Familiarity)
                .Take(take)
                .ToListAsync();
        }
    }
}
