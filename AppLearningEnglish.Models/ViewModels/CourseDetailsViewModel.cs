namespace AppLearningEnglish.Models.ViewModels
{
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; } = null!;

        public bool IsEnrolled { get; set; }

        public decimal Progress { get; set; }

        public List<Lesson> Lessons { get; set; } = new();

        public bool IsLocked { get; set; }

        public bool IsRecommended { get; set; }

        public string? UserLevel { get; set; }
    }
}
