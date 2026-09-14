namespace AppLearningEnglish.Models.ViewModels
{
    public class ListeningPlayViewModel
    {
        public Lesson Lesson { get; set; } = null!;

        public ListeningLesson ListeningLesson { get; set; } = null!;

        public List<ListeningQuestion> Questions { get; set; } = new();
    }

    public class ListeningResultItemViewModel
    {
        public int QuestionId { get; set; }

        public string Question { get; set; } = string.Empty;

        public string ExpectedAnswer { get; set; } = string.Empty;

        public string UserAnswer { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public int? AccuracyPercent { get; set; }
    }

    public class ListeningResultViewModel
    {
        public Lesson Lesson { get; set; } = null!;

        public ListeningLesson ListeningLesson { get; set; } = null!;

        public List<ListeningResultItemViewModel> Results { get; set; } = new();

        public int CorrectCount { get; set; }

        public int TotalQuestions { get; set; }

        public int PointsEarned { get; set; }
    }

    public class LessonDetailsViewModel
    {
        public Lesson Lesson { get; set; } = null!;

        public UserLesson? Progress { get; set; }

        public bool HasListening { get; set; }

        public bool HasExercise { get; set; }

        public int WordCount { get; set; }

        public string? YoutubeEmbedUrl { get; set; }

        public bool CanComplete { get; set; }

        public bool VocabDone { get; set; }

        public bool ListeningDone { get; set; }

        public bool ExerciseDone { get; set; }

        public bool ReadingDone { get; set; }

        public bool SpeakingDone { get; set; }

        public bool WritingDone { get; set; }

        public bool GrammarDone { get; set; }

        public bool HasReading { get; set; }

        public bool HasSpeaking { get; set; }

        public bool HasWriting { get; set; }

        public bool HasGrammar { get; set; }

        public List<string> MissingSteps { get; set; } = new();

        public string? NoteBody { get; set; }

        public bool IsBookmarked { get; set; }
    }

    public class LessonWordListViewModel
    {
        public Lesson Lesson { get; set; } = null!;

        public List<LessonWordItemViewModel> Words { get; set; } = new();
    }

    public class LessonWordItemViewModel
    {
        public Word Word { get; set; } = null!;

        public bool IsSaved { get; set; }
    }
}
