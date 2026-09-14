namespace AppLearningEnglish.Models.ViewModels
{
    public class DailyStreakVM
    {
        // ================================
        // TODAY
        // ================================

        public DateOnly Today { get; set; }

        public int TodayMinutes { get; set; }

        public int TodayPoints { get; set; }


        // ================================
        // STREAK
        // ================================

        public int CurrentStreak { get; set; }

        public int LongestStreak { get; set; }


        // ================================
        // STATISTICS
        // ================================

        public int TotalStudyDays { get; set; }

        public int TotalStudyMinutes { get; set; }

        public int TotalPoints { get; set; }


        // ================================
        // DAILY GOAL
        // ================================

        public int DailyGoalMinutes { get; set; }

        public int DailyGoalProgress { get; set; }


        // ================================
        // TODAY STUDIED
        // ================================

        public bool StudiedToday { get; set; }


        // ================================
        // CALENDAR
        // ================================

        public int CalendarYear { get; set; }

        public int CalendarMonth { get; set; }

        public List<CalendarDayVM> CalendarDays { get; set; }
            = new List<CalendarDayVM>();


        // ================================
        // HISTORY
        // ================================

        public List<DailyStreakHistoryVM> History { get; set; }
            = new List<DailyStreakHistoryVM>();
    }


    public class CalendarDayVM
    {
        public DateOnly Date { get; set; }

        public bool HasStudied { get; set; }

        public int MinutesStudied { get; set; }

        public int PointsEarned { get; set; }
    }


    public class DailyStreakHistoryVM
    {
        public DateOnly StudyDate { get; set; }

        public int MinutesStudied { get; set; }

        public int PointsEarned { get; set; }
    }
}