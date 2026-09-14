namespace AppLearningEnglish.Models.ViewModels
{
    public class SpeakingPracticeViewModel
    {
        public Word Word { get; set; } = null!;

        public string? HeardText { get; set; }

        public int? ScorePercent { get; set; }

        public bool? IsPassed { get; set; }

        public int PointsEarned { get; set; }

        public int? DurationMs { get; set; }

        public SpeechAssessment? Assessment { get; set; }
    }
}
