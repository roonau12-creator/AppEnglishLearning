namespace AppLearningEnglish.Models.ViewModels
{
    public class StudyReminderInboxViewModel
    {
        public int DueWordCount { get; set; }

        public int ReviewLimit { get; set; }

        public bool StreakAtRisk { get; set; }

        public int CurrentStreak { get; set; }

        public bool StudiedToday { get; set; }

        public int ItemCount =>
            (DueWordCount > 0 ? 1 : 0) + (StreakAtRisk ? 1 : 0);
    }

    public class StudyReminderCandidate
    {
        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int DueWordCount { get; set; }

        public int ReviewLimit { get; set; }

        public bool StreakAtRisk { get; set; }
    }
}
