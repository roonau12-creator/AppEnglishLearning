using AppLearningEnglish.Models;

namespace AppLearningEnglish.Models.ViewModels
{
    public class HomePageViewModel
    {
        public bool IsSignedIn { get; set; }

        public TodayDashboardViewModel? Today { get; set; }

        public List<Course> FeaturedCourses { get; set; } = new();
    }

    public class TodayDashboardViewModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public string? EnglishLevel { get; set; }

        public int MinutesToday { get; set; }

        public int DailyGoalMinutes { get; set; }

        public int GoalPercent { get; set; }

        public int DueWordCount { get; set; }

        public int ReviewLimit { get; set; }

        public int CurrentStreak { get; set; }

        public bool StreakAtRisk { get; set; }

        public bool StudiedToday { get; set; }

        public UserLesson? ContinueLesson { get; set; }

        public List<UserCourse> ActiveCourses { get; set; } = new();

        public List<UserVocabulary> WeakWords { get; set; } = new();

        public int UnreadReminders { get; set; }

        public int UnreadNotifications { get; set; }

        public int WordsLearned { get; set; }

        public int VocabGoalTotal { get; set; } = 2000;

        public decimal AverageScore { get; set; }

        public int Coins { get; set; }

        public int PlayerXp { get; set; }

        public int PlayerLevel { get; set; } = 1;

        public string LearningGoalKind { get; set; } = "vocab";

        public decimal? IeltsBandTarget { get; set; }

        public List<LeaderboardRowViewModel> Leaderboard { get; set; } = new();

        public List<TodayPlanItemViewModel> TodayPlan { get; set; } = new();

        public List<UserChallenge> Challenges { get; set; } = new();
    }

    public class TodayPlanItemViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Controller { get; set; } = string.Empty;

        public string Action { get; set; } = "Index";

        public string? RouteMode { get; set; }

        public string Icon { get; set; } = "bi-book";
    }

    public class LeaderboardRowViewModel
    {
        public string Name { get; set; } = string.Empty;

        public int Points { get; set; }

        public bool IsMe { get; set; }
    }

    public class PlayerStatsViewModel
    {
        public int Coins { get; set; }

        public int PlayerXp { get; set; }

        public int PlayerLevel { get; set; } = 1;

        public int XpIntoLevel { get; set; }

        public int XpForNext { get; set; } = 100;
    }

    public class VocabQuizViewModel
    {
        public string Mode { get; set; } = "type";

        public UserVocabulary? Item { get; set; }

        public List<string> Choices { get; set; } = new();

        public string? UserAnswer { get; set; }

        public bool? IsCorrect { get; set; }

        public int Remaining { get; set; }
    }

    public class ReadingPlayViewModel
    {
        public Lesson Lesson { get; set; } = null!;

        public List<ReadingQuizItem> Questions { get; set; } = new();

        public int? Score { get; set; }

        public int? Total { get; set; }
    }

    public class ReadingQuizItem
    {
        public int WordId { get; set; }

        public string Prompt { get; set; } = string.Empty;

        public List<string> Options { get; set; } = new();

        public string Answer { get; set; } = string.Empty;

        public string? UserAnswer { get; set; }

        public bool? IsCorrect { get; set; }
    }

    public class WritingPracticeViewModel
    {
        public WordExample? Example { get; set; }

        public WritingPrompt? FreePrompt { get; set; }

        public string Prompt { get; set; } = string.Empty;

        public string Expected { get; set; } = string.Empty;

        public string? UserAnswer { get; set; }

        public int? ScorePercent { get; set; }

        public bool? IsPassed { get; set; }

        public WritingReview? Review { get; set; }

        public bool IsFreeWrite => FreePrompt != null;
    }

    public class WritingReview
    {
        public int Overall { get; set; }

        public int Grammar { get; set; }

        public int Meaning { get; set; }

        public int Coverage { get; set; }

        public List<string> Corrections { get; set; } = new();

        public List<string> Comments { get; set; } = new();

        public List<WritingDiffWord> Diff { get; set; } = new();

        public string? SuggestedRewrite { get; set; }
    }

    public class WritingDiffWord
    {
        public string Text { get; set; } = string.Empty;

        public string Status { get; set; } = "match";

        public string? Expected { get; set; }
    }

    public class WritingHubViewModel
    {
        public List<WordExample> RewriteItems { get; set; } = new();

        public List<WritingPrompt> Prompts { get; set; } = new();

        public string? Genre { get; set; }
    }

    public class GrammarDrillViewModel
    {
        public Lesson Lesson { get; set; } = null!;

        public List<GrammarDrillItem> Items { get; set; } = new();

        public int? CorrectCount { get; set; }
    }

    public class GrammarPracticeViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Kind { get; set; } = string.Empty;

        public List<GrammarDrillItem> Items { get; set; } = new();

        public int? CorrectCount { get; set; }
    }

    public class GrammarDrillItem
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public string? Explanation { get; set; }

        public List<string> Options { get; set; } = new();

        public string Answer { get; set; } = string.Empty;

        public string? UserAnswer { get; set; }

        public bool? IsCorrect { get; set; }
    }

    public class ConversationPlayViewModel
    {
        public ConversationScenario Scenario { get; set; } = null!;

        public ConversationTurn Current { get; set; } = null!;

        public int TurnIndex { get; set; }

        public string? Reply { get; set; }

        public int? Score { get; set; }

        public bool? IsPassed { get; set; }

        public bool IsLast { get; set; }
    }

    public class ConversationScenario
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Level { get; set; } = "Beginner";

        public string Setting { get; set; } = string.Empty;

        public IReadOnlyList<ConversationTurn> Turns { get; set; } = [];
    }

    public class ConversationTurn
    {
        public string Tutor { get; set; } = string.Empty;

        public string Hint { get; set; } = string.Empty;

        public string[] Keywords { get; set; } = [];

        public string Sample { get; set; } = string.Empty;
    }

    public class PlacementQuestion
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public string[] Options { get; set; } = Array.Empty<string>();

        public string Answer { get; set; } = string.Empty;
    }

    public class PlacementResultViewModel
    {
        public int Score { get; set; }

        public int Total { get; set; }

        public string Level { get; set; } = string.Empty;

        public List<Course> RecommendedCourses { get; set; } = new();

        public List<Course> LockedCourses { get; set; } = new();
    }

    public class SkillReportViewModel
    {
        public int TotalWords { get; set; }

        public int MasteredWords { get; set; }

        public int DueWords { get; set; }

        public int WeakWords { get; set; }

        public decimal VocabPercent { get; set; }

        public int ListeningDone { get; set; }

        public int LessonsStarted { get; set; }

        public decimal ExerciseAverage { get; set; }

        public int ExerciseAttempts { get; set; }

        public int MinutesToday { get; set; }

        public int CurrentStreak { get; set; }

        public List<UserVocabulary> WeakList { get; set; } = new();

        public decimal GrammarPercent { get; set; }

        public decimal ListeningPercent { get; set; }

        public decimal SpeakingPercent { get; set; }

        public decimal ReadingPercent { get; set; }

        public decimal WritingPercent { get; set; }

        public List<ProgressPointViewModel> DailyPoints { get; set; } = new();

        public List<ProgressPointViewModel> WeeklyPoints { get; set; } = new();

        public List<ProgressPointViewModel> MonthlyPoints { get; set; } = new();
    }

    public class ProgressPointViewModel
    {
        public string Label { get; set; } = string.Empty;

        public int Minutes { get; set; }

        public int Xp { get; set; }

        public int QuizScore { get; set; }

        public int WordsLearned { get; set; }
    }

    public class SpeakingPromptViewModel
    {
        public string Mode { get; set; } = "sentence";

        public string Title { get; set; } = string.Empty;

        public string Expected { get; set; } = string.Empty;

        public string Hint { get; set; } = string.Empty;

        public string? Pronunciation { get; set; }

        public string? HeardText { get; set; }

        public int? DurationMs { get; set; }

        public int? ScorePercent { get; set; }

        public bool? IsPassed { get; set; }

        public SpeechAssessment? Assessment { get; set; }
    }

    public class SpeechAssessment
    {
        public int Overall { get; set; }

        public int Accuracy { get; set; }

        public int Completeness { get; set; }

        public int Fluency { get; set; }

        public int Stress { get; set; }

        public int Intonation { get; set; }

        public List<SpeechWordMark> Words { get; set; } = new();

        public List<string> Tips { get; set; } = new();
    }

    public class SpeechWordMark
    {
        public string Expected { get; set; } = string.Empty;

        public string? Heard { get; set; }

        public string Status { get; set; } = "match";

        public int Distance { get; set; }

        public int Similarity { get; set; }

        public string? PhoneticHint { get; set; }
    }

    public class ReadingHubViewModel
    {
        public List<ReadingPassage> Passages { get; set; } = new();

        public List<Lesson> LessonReadings { get; set; } = new();
    }

    public class ReadingPassagePlayViewModel
    {
        public ReadingPassage Passage { get; set; } = null!;

        public List<ReadingPassageItem> Questions { get; set; } = new();

        public List<string> LearnedWords { get; set; } = new();

        public int? Score { get; set; }

        public int? Total { get; set; }
    }

    public class ReadingPassageItem
    {
        public int QuestionId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Prompt { get; set; } = string.Empty;

        public List<string> Options { get; set; } = new();

        public string Answer { get; set; } = string.Empty;

        public string? Explanation { get; set; }

        public string? UserAnswer { get; set; }

        public bool? IsCorrect { get; set; }
    }

    public class GrammarHubViewModel
    {
        public string? UserLevel { get; set; }

        public List<GrammarTopic> Topics { get; set; } = new();

        public List<Lesson> LessonDrills { get; set; } = new();

        public IReadOnlyDictionary<int, GrammarProgress> Progress { get; set; }
            = new Dictionary<int, GrammarProgress>();
    }

    public class GrammarTopicViewModel
    {
        public GrammarTopic Topic { get; set; } = null!;

        public List<GrammarDrillItem> Items { get; set; } = new();

        public int? CorrectCount { get; set; }
    }

    public class CourseCatalogItemViewModel
    {
        public Course Course { get; set; } = null!;

        public bool IsRecommended { get; set; }

        public bool IsLocked { get; set; }
    }

    public class ReminderSettingsViewModel
    {
        public bool ReminderEnabled { get; set; } = true;

        public int ReminderHour { get; set; } = 19;

        public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";
    }

    public class ReminderPageViewModel
    {
        public ReminderSettingsViewModel Settings { get; set; } = new();

        public IReadOnlyList<StudyReminderLog> Logs { get; set; } = Array.Empty<StudyReminderLog>();
    }
}
