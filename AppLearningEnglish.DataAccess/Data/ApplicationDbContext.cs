using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;
namespace AppLearningEnglish.DataAccess.Data
{
    public class ApplicationDbContext:IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Topic> topics { get; set; }
        public DbSet<Course> courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Word> words { get; set; }
        public DbSet<WordMeaning> wordMeanings { get; set; }
        public DbSet<WordExample> wordExamples { get; set; }
        public DbSet<Exercise> exercises { get; set; }
        public DbSet<Question> questions { get; set; }
        public DbSet<Answer> answers { get; set; }
        public DbSet<ListeningLesson> ListeningLessons { get; set; }
        public DbSet<ListeningQuestion> ListeningQuestions { get; set; }
        public DbSet<ApplicationUser> applicationUsers { get; set; }
        public DbSet<UserLesson> UserLessons { get; set; }
        public DbSet<UserVocabulary> UserVocabularies { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }
        public DbSet<ExerciseAttempt> ExerciseAttempts { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<UserPoint> UserPoints { get; set; }
        public DbSet<DailyStreak> DailyStreaks { get; set; }
        public DbSet<StudyReminderLog> StudyReminderLogs { get; set; }
        public DbSet<UserNote> UserNotes { get; set; }
        public DbSet<UserBookmark> UserBookmarks { get; set; }
        public DbSet<ReadingPassage> ReadingPassages { get; set; }
        public DbSet<ReadingQuestion> ReadingQuestions { get; set; }
        public DbSet<GrammarTopic> GrammarTopics { get; set; }
        public DbSet<GrammarItem> GrammarItems { get; set; }
        public DbSet<WritingPrompt> WritingPrompts { get; set; }
        public DbSet<PracticeAttempt> PracticeAttempts { get; set; }
        public DbSet<PlacementItem> PlacementItems { get; set; }
        public DbSet<InAppNotification> InAppNotifications { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<UserChallenge> UserChallenges { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<ForumComment> ForumComments { get; set; }
        public DbSet<ForumFollow> ForumFollows { get; set; }
        public DbSet<QuizMistake> QuizMistakes { get; set; }
        public DbSet<GrammarProgress> GrammarProgresses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ListeningLesson>()
                .HasOne(x => x.Lesson)
                .WithOne(x => x.ListeningLesson)
                .HasForeignKey<ListeningLesson>(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ListeningLesson>()
                .HasIndex(x => x.LessonId)
                .IsUnique();

            modelBuilder.Entity<Word>()
                .HasOne(x => x.Lesson)
                .WithMany(x => x.Words)
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserLesson>()
                .HasOne(x => x.Lesson)
                .WithMany()
                .HasForeignKey(x => x.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLesson>()
                .HasIndex(x => new
                {
                    x.ApplicationUserId,
                    x.LessonId
                })
                .IsUnique();
            modelBuilder.Entity<UserLesson>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserVocabulary>()
                .HasOne(x => x.Word)
                .WithMany()
                .HasForeignKey(x => x.WordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserVocabulary>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserVocabulary>()
                .HasIndex(x => new
                {
                    x.ApplicationUserId,
                    x.WordId
                })
                .IsUnique();
            modelBuilder.Entity<UserCourse>()
                .HasOne(x => x.Course)
                .WithMany()
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCourse>()
                .HasIndex(x => new
                {
                    x.ApplicationUserId,
                    x.CourseId
                })
                .IsUnique();
            modelBuilder.Entity<ExerciseAttempt>()
                .HasOne(x => x.Exercise)
                .WithMany()
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExerciseAttempt>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExerciseAttempt>()
                .HasIndex(x => new
                {
                    x.ApplicationUserId,
                    x.ExerciseId
                });

            modelBuilder.Entity<UserAchievement>()
                .HasOne(x => x.Achievement)
                .WithMany()
                .HasForeignKey(x => x.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAchievement>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAchievement>()
                .HasIndex(x => new
                {
                    x.UserId,
                    x.AchievementId
                })
                .IsUnique();

            modelBuilder.Entity<UserPoint>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPoint>()
                .HasIndex(x => x.UserId)
                .IsUnique();
            modelBuilder.Entity<DailyStreak>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyStreak>()
                .HasIndex(x => new
                {
                    x.ApplicationUserId,
                    x.StudyDate
                })
                .IsUnique();

            modelBuilder.Entity<StudyReminderLog>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserNote>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBookmark>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBookmark>()
                .HasIndex(x => new { x.ApplicationUserId, x.LessonId })
                .IsUnique();

            modelBuilder.Entity<ReadingQuestion>()
                .HasOne(x => x.Passage)
                .WithMany(x => x.Questions)
                .HasForeignKey(x => x.ReadingPassageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrammarItem>()
                .HasOne(x => x.Topic)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.GrammarTopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PracticeAttempt>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lesson>()
                .HasOne(x => x.ReadingPassage)
                .WithMany()
                .HasForeignKey(x => x.ReadingPassageId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Lesson>()
                .HasOne(x => x.GrammarTopic)
                .WithMany()
                .HasForeignKey(x => x.GrammarTopicId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Lesson>()
                .HasOne(x => x.WritingPrompt)
                .WithMany()
                .HasForeignKey(x => x.WritingPromptId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InAppNotification>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChallenge>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChallenge>()
                .HasOne(x => x.Challenge)
                .WithMany(x => x.UserChallenges)
                .HasForeignKey(x => x.ChallengeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserChallenge>()
                .HasIndex(x => new { x.ApplicationUserId, x.ChallengeId, x.PeriodKey })
                .IsUnique();

            modelBuilder.Entity<ForumPost>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumComment>()
                .HasOne(x => x.Post)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.ForumPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumComment>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumFollow>()
                .HasOne(x => x.Follower)
                .WithMany()
                .HasForeignKey(x => x.FollowerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumFollow>()
                .HasOne(x => x.FollowedUser)
                .WithMany()
                .HasForeignKey(x => x.FollowedUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ForumFollow>()
                .HasIndex(x => new { x.FollowerId, x.FollowedUserId })
                .IsUnique();

            modelBuilder.Entity<QuizMistake>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrammarProgress>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrammarProgress>()
                .HasOne(x => x.Topic)
                .WithMany()
                .HasForeignKey(x => x.GrammarTopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GrammarProgress>()
                .HasIndex(x => new { x.ApplicationUserId, x.GrammarTopicId })
                .IsUnique();

        }
            
    }
}