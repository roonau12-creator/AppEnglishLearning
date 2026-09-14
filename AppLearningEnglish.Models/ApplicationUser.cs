using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AppLearningEnglish.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        public string? AvatarUrl { get; set; }

        public string? EnglishLevel { get; set; }

        [Range(5, 480)]
        public int DailyGoalMinutes { get; set; } = 30;

        [Range(1, 200)]
        public int VocabDailyTarget { get; set; } = 10;

        /// <summary>
        /// Nhân khoảng cách ôn của SRS: 50 = ôn dày gấp đôi, 200 = ôn thưa một nửa.
        /// </summary>
        [Range(25, 400)]
        public int ReviewIntervalPercent { get; set; } = 100;

        /// <summary>
        /// Lần gần nhất đã gửi email nhắc ôn từ / streak.
        /// </summary>
        public DateTime? LastStudyReminderAt { get; set; }

        public bool ReminderEnabled { get; set; } = true;

        [Range(0, 23)]
        public int ReminderHour { get; set; } = 19;

        [MaxLength(80)]
        public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";

        [MaxLength(20)]
        public string PreferredTheme { get; set; } = "light";

        [MaxLength(20)]
        public string LearningGoalKind { get; set; } = "vocab";

        [Range(50, 20000)]
        public int VocabGoalTotal { get; set; } = 2000;

        [Range(0, 9)]
        public decimal? IeltsBandTarget { get; set; }

        public int Coins { get; set; }

        public int PlayerXp { get; set; }

        public int PlayerLevel { get; set; } = 1;

        [MaxLength(8)]
        public string UiLanguage { get; set; } = "vi";

        public DateTime? LastWeeklySummaryAt { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}
