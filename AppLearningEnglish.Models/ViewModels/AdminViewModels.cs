namespace AppLearningEnglish.Models.ViewModels
{
    public class AdminUserDetailViewModel
    {
        public ApplicationUser User { get; set; } = null!;

        public List<string> Roles { get; set; } = new();

        public bool IsLocked { get; set; }

        public int Points { get; set; }

        public int CurrentStreak { get; set; }

        public int TotalStudyMinutes { get; set; }

        public List<UserCourse> Enrollments { get; set; } = new();

        public List<UserLesson> Lessons { get; set; } = new();

        public List<ExerciseAttempt> Attempts { get; set; } = new();

        public List<PracticeAttempt> PracticeAttempts { get; set; } = new();

        public List<Course> AvailableCourses { get; set; } = new();
    }

    public class AdminDashboardViewModel
    {
        public int Users { get; set; }

        public int Courses { get; set; }

        public int Lessons { get; set; }

        public int Enrollments { get; set; }

        public int Attempts { get; set; }

        public int Words { get; set; }

        public int CompletedEnrollments { get; set; }

        public decimal CompletionRate { get; set; }

        public decimal AverageScore { get; set; }

        public int ActiveLearnersToday { get; set; }

        /// <summary>
        /// Số lượt đăng ký theo ngày, 14 ngày gần nhất.
        /// </summary>
        public List<ChartPointViewModel> EnrollmentTrend { get; set; } = new();

        /// <summary>
        /// Số lượt làm bài theo ngày, 14 ngày gần nhất.
        /// </summary>
        public List<ChartPointViewModel> AttemptTrend { get; set; } = new();

        public List<CourseStatViewModel> TopCourses { get; set; } = new();
    }

    public class ChartPointViewModel
    {
        public string Label { get; set; } = string.Empty;

        public int Value { get; set; }
    }

    public class CourseStatViewModel
    {
        public string CourseName { get; set; } = string.Empty;

        public int Enrollments { get; set; }

        public decimal AverageProgress { get; set; }
    }
}
