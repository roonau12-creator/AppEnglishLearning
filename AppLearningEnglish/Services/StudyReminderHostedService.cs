using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Services
{
    public class StudyReminderHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<StudyReminderHostedService> _logger;

        public StudyReminderHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<StudyReminderHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await SendDueRemindersAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Gửi email nhắc học thất bại.");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Host đang tắt — không để TaskCanceledException làm sập app.
            }
        }

        private async Task SendDueRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var reminders = scope.ServiceProvider.GetRequiredService<IStudyReminderService>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IAppEmailSender>();

            var candidates = await reminders.GetDueEmailCandidatesAsync(stoppingToken);
            var sentIds = new List<string>();

            foreach (var candidate in candidates)
            {
                stoppingToken.ThrowIfCancellationRequested();

                var parts = new List<string>
                {
                    $"<p>Xin chào {candidate.DisplayName},</p>"
                };

                if (candidate.DueWordCount > 0)
                {
                    parts.Add(
                        $"<p>Bạn có <strong>{candidate.DueWordCount}</strong> từ đến hạn ôn. " +
                        $"Hôm nay ôn tối đa {candidate.ReviewLimit} từ theo mục tiêu hồ sơ.</p>");
                }

                if (candidate.StreakAtRisk)
                {
                    parts.Add("<p>Chuỗi ngày học của bạn sắp gãy nếu hôm nay chưa học.</p>");
                }

                parts.Add("<p>Mở app để ôn từ hoặc học một bài ngắn.</p>");

                var sent = await emailSender.SendAsync(
                    candidate.Email,
                    "Nhắc học AppLearningEnglish",
                    string.Join(string.Empty, parts));

                if (sent || !emailSender.IsConfigured)
                {
                    sentIds.Add(candidate.UserId);
                    var logParts = new List<string>();

                    if (candidate.DueWordCount > 0)
                    {
                        logParts.Add($"{candidate.DueWordCount} từ đến hạn");
                    }

                    if (candidate.StreakAtRisk)
                    {
                        logParts.Add("chuỗi ngày học sắp gãy");
                    }

                    await reminders.LogAsync(
                        candidate.UserId,
                        "email",
                        "Đã gửi email nhắc học",
                        string.Join(" · ", logParts));
                }
            }

            await reminders.MarkRemindedAsync(sentIds, DateTime.UtcNow);
            _logger.LogInformation("Đã xử lý {Count} email nhắc học.", sentIds.Count);

            await SendWeeklySummariesAsync(scope, stoppingToken);
        }

        private async Task SendWeeklySummariesAsync(IServiceScope scope, CancellationToken stoppingToken)
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var workspace = scope.ServiceProvider.GetRequiredService<ILearnerWorkspaceService>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IAppEmailSender>();
            var reminders = scope.ServiceProvider.GetRequiredService<IStudyReminderService>();
            var cutoff = DateTime.UtcNow.AddDays(-7);
            if (DateTime.UtcNow.DayOfWeek != DayOfWeek.Sunday)
            {
                return;
            }

            var users = await db.applicationUsers
                .Where(x => x.ReminderEnabled
                    && x.Email != null
                    && (x.LastWeeklySummaryAt == null || x.LastWeeklySummaryAt < cutoff))
                .Take(25)
                .ToListAsync(stoppingToken);

            foreach (var user in users)
            {
                stoppingToken.ThrowIfCancellationRequested();
                var skills = await workspace.GetSkillsAsync(user.Id);
                var body =
                    $"<p>Xin chào {user.FullName ?? user.Email},</p>" +
                    $"<p>Tóm tắt tuần: streak {skills.CurrentStreak} ngày, " +
                    $"{skills.MasteredWords}/{skills.TotalWords} từ thuộc, quiz TB {skills.ExerciseAverage}%.</p>" +
                    "<p>Hãy mở app để ôn flashcard hoặc làm Final Test.</p>";
                var sent = await emailSender.SendAsync(
                    user.Email!,
                    "Tổng kết tuần AppLearningEnglish",
                    body);
                if (sent || !emailSender.IsConfigured)
                {
                    user.LastWeeklySummaryAt = DateTime.UtcNow;
                    await reminders.LogAsync(user.Id, "email", "Tổng kết tuần", "Đã gửi email tổng kết tuần");
                }
            }

            await db.SaveChangesAsync(stoppingToken);
        }
    }
}
