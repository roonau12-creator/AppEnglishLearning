using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Identity
{
    public static class ApplicationDataSeeder
    {
        private const string DemoEmail = "user@applearning.local";
        private const string DemoPassword = "User123";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();
            var env = scope.ServiceProvider
                .GetRequiredService<IWebHostEnvironment>();

            await context.Database.MigrateAsync();

            if (await context.topics.AnyAsync())
            {
                await SeedCatalog.EnrichAsync(context, env.WebRootPath);

                var existingDemoUser = await userManager.FindByEmailAsync(DemoEmail);

                if (existingDemoUser != null)
                {
                    await SeedCatalog.EnsureDailyStreaksAsync(context, existingDemoUser.Id);
                }

                return;
            }

            var now = DateTime.UtcNow;

            var greetings = new Topic
            {
                Name = "Greetings",
                Description = "Chào hỏi và giới thiệu bản thân"
            };
            var dailyLife = new Topic
            {
                Name = "Daily Life",
                Description = "Hoạt động thường ngày"
            };
            var travel = new Topic
            {
                Name = "Travel",
                Description = "Du lịch, sân bay và hỏi đường"
            };
            var food = new Topic
            {
                Name = "Food",
                Description = "Đồ ăn, thức uống và nhà hàng"
            };

            context.topics.AddRange(greetings, dailyLife, travel, food);
            await context.SaveChangesAsync();

            var beginner = new Course
            {
                Name = "English for Beginners",
                Description = "Khóa học nền tảng cho người mới bắt đầu.",
                Level = "Beginner",
                ThumbnailUrl = "/images/courses/english-for-beginners.svg",
                IsPublished = true,
                CreatedAt = now.AddDays(-10)
            };
            var conversation = new Course
            {
                Name = "Daily Conversation",
                Description = "Luyện nói những tình huống giao tiếp hàng ngày.",
                Level = "Elementary",
                ThumbnailUrl = "/images/courses/daily-conversation.svg",
                IsPublished = true,
                CreatedAt = now.AddDays(-7)
            };
            var travelCourse = new Course
            {
                Name = "Travel English",
                Description = "Tiếng Anh sân bay, khách sạn và hỏi đường.",
                Level = "Intermediate",
                ThumbnailUrl = "/images/courses/travel-english.svg",
                IsPublished = true,
                CreatedAt = now.AddDays(-3)
            };

            context.courses.AddRange(beginner, conversation, travelCourse);
            await context.SaveChangesAsync();

            var lessonHello = new Lesson
            {
                CourseId = beginner.Id,
                TopicId = greetings.Id,
                Title = "Hello and Introductions",
                Description = "Cách chào hỏi và giới thiệu tên.",
                Content = LessonContents.Hello,
                GrammarNotes = LessonGrammar.Hello,
                VideoUrl = "https://www.youtube.com/watch?v=gVfgkFaswn4",
                LessonOrder = 1,
                DurationMinutes = 20,
                IsPublished = true
            };
            var lessonNumbers = new Lesson
            {
                CourseId = beginner.Id,
                TopicId = dailyLife.Id,
                Title = "Numbers and Time",
                Description = "Số đếm và hỏi giờ.",
                Content = LessonContents.Numbers,
                GrammarNotes = LessonGrammar.Numbers,
                VideoUrl = "https://www.youtube.com/watch?v=DR-cfDsHCGA",
                LessonOrder = 2,
                DurationMinutes = 25,
                IsPublished = true
            };
            var lessonCafe = new Lesson
            {
                CourseId = conversation.Id,
                TopicId = food.Id,
                Title = "At the Cafe",
                Description = "Gọi món tại quán cà phê.",
                Content = LessonContents.Cafe,
                GrammarNotes = LessonGrammar.Cafe,
                VideoUrl = "https://www.youtube.com/watch?v=KYkl5eYbcVY",
                LessonOrder = 1,
                DurationMinutes = 30,
                IsPublished = true
            };
            var lessonFriends = new Lesson
            {
                CourseId = conversation.Id,
                TopicId = dailyLife.Id,
                Title = "Meeting Friends",
                Description = "Nói chuyện khi gặp bạn bè.",
                Content = LessonContents.Friends,
                GrammarNotes = LessonGrammar.Friends,
                VideoUrl = "https://www.youtube.com/watch?v=oxNo1uvoc-I",
                LessonOrder = 2,
                DurationMinutes = 25,
                IsPublished = true
            };
            var lessonAirport = new Lesson
            {
                CourseId = travelCourse.Id,
                TopicId = travel.Id,
                Title = "At the Airport",
                Description = "Check-in và hỏi cổng lên máy bay.",
                Content = LessonContents.Airport,
                GrammarNotes = LessonGrammar.Airport,
                VideoUrl = "https://www.youtube.com/watch?v=l8e5CKs5z-g",
                LessonOrder = 1,
                DurationMinutes = 35,
                IsPublished = true
            };
            var lessonDirections = new Lesson
            {
                CourseId = travelCourse.Id,
                TopicId = travel.Id,
                Title = "Asking for Directions",
                Description = "Hỏi đường khi đi du lịch.",
                Content = LessonContents.Directions,
                GrammarNotes = LessonGrammar.Directions,
                VideoUrl = "https://www.youtube.com/watch?v=kYXieg-sCsM",
                LessonOrder = 2,
                DurationMinutes = 30,
                IsPublished = true
            };

            context.Lessons.AddRange(
                lessonHello,
                lessonNumbers,
                lessonCafe,
                lessonFriends,
                lessonAirport,
                lessonDirections);
            await context.SaveChangesAsync();

            var achStart = new Achievement
            {
                Name = "First Step",
                Description = "Bắt đầu hành trình học tiếng Anh.",
                RequiredPoints = 0
            };
            var achLearner = new Achievement
            {
                Name = "Dedicated Learner",
                Description = "Đạt 50 điểm thưởng.",
                RequiredPoints = 50
            };
            var achMaster = new Achievement
            {
                Name = "Vocabulary Master",
                Description = "Đạt 200 điểm thưởng.",
                RequiredPoints = 200
            };

            context.Achievements.AddRange(achStart, achLearner, achMaster);
            await context.SaveChangesAsync();

            var demoUser = await userManager.FindByEmailAsync(DemoEmail);

            if (demoUser == null)
            {
                demoUser = new ApplicationUser
                {
                    UserName = DemoEmail,
                    Email = DemoEmail,
                    FullName = "Nguyen Van A",
                    EnglishLevel = "Beginner",
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    CreatedAt = now
                };

                var createUser = await userManager.CreateAsync(demoUser, DemoPassword);

                if (!createUser.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join("; ", createUser.Errors.Select(x => x.Description)));
                }

                await userManager.AddToRoleAsync(demoUser, AppRoles.User);
            }

            await SeedCatalog.EnrichAsync(context, env.WebRootPath);

            var userId = demoUser.Id;

            await SeedCatalog.EnsureDailyStreaksAsync(context, userId);

            var helloLesson = await context.Lessons.FirstAsync(x => x.Title == "Hello and Introductions");
            var numbersLesson = await context.Lessons.FirstAsync(x => x.Title == "Numbers and Time");
            var helloWord = await context.words.FirstAsync(x => x.WordText.ToLower() == "hello");
            var nameWord = await context.words.FirstAsync(x => x.WordText.ToLower() == "name");
            var coffeeWord = await context.words.FirstAsync(x => x.WordText.ToLower() == "coffee");
            var helloExercise = await context.exercises.FirstAsync(x => x.LessonId == helloLesson.Id);

            if (!await context.UserCourses.AnyAsync(x => x.ApplicationUserId == userId))
            {
                context.UserCourses.Add(new UserCourse
                {
                    ApplicationUserId = userId,
                    CourseId = beginner.Id,
                    Progress = 50,
                    EnrolledAt = now.AddDays(-2)
                });

                context.UserLessons.AddRange(
                    new UserLesson
                    {
                        ApplicationUserId = userId,
                        LessonId = helloLesson.Id,
                        IsCompleted = true,
                        Progress = 100,
                        VocabDone = true,
                        ListeningDone = true,
                        ExerciseDone = true,
                        CompletedAt = now.AddDays(-1)
                    },
                    new UserLesson
                    {
                        ApplicationUserId = userId,
                        LessonId = numbersLesson.Id,
                        IsCompleted = false,
                        Progress = 40
                    });

                context.UserVocabularies.AddRange(
                    new UserVocabulary
                    {
                        ApplicationUserId = userId,
                        WordId = helloWord.Id,
                        Status = "Mastered",
                        Familiarity = 5,
                        CorrectCount = 8,
                        WrongCount = 1,
                        LastReviewedAt = now.AddHours(-6),
                        NextReviewAt = now.AddDays(7)
                    },
                    new UserVocabulary
                    {
                        ApplicationUserId = userId,
                        WordId = nameWord.Id,
                        Status = "Learning",
                        Familiarity = 2,
                        CorrectCount = 3,
                        WrongCount = 2,
                        LastReviewedAt = now.AddHours(-2),
                        NextReviewAt = now.AddHours(-1)
                    },
                    new UserVocabulary
                    {
                        ApplicationUserId = userId,
                        WordId = coffeeWord.Id,
                        Status = "Reviewing",
                        Familiarity = 3,
                        CorrectCount = 4,
                        WrongCount = 1,
                        LastReviewedAt = now.AddDays(-1),
                        NextReviewAt = now.AddHours(8)
                    });

                context.ExerciseAttempts.Add(new ExerciseAttempt
                {
                    ApplicationUserId = userId,
                    ExerciseId = helloExercise.Id,
                    Score = 100,
                    CorrectAnswers = 2,
                    TotalQuestions = 2,
                    StartedAt = now.AddDays(-1),
                    CompletedAt = now.AddDays(-1).AddMinutes(8)
                });

                context.UserPoints.Add(new UserPoint
                {
                    UserId = userId,
                    Points = 50,
                    UpdatedAt = now
                });

                context.UserAchievements.AddRange(
                    new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achStart.Id,
                        UnlockedAt = now.AddDays(-2)
                    },
                    new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achLearner.Id,
                        UnlockedAt = now
                    });

                await context.SaveChangesAsync();
            }
        }
    }

    internal static class LessonContents
    {
        public const string Hello =
            "When you meet someone, you can say Hello or Hi.\n\n" +
            "A: Hello! My name is Anna.\n" +
            "B: Hi, Anna. My name is Minh. Nice to meet you.\n" +
            "A: Nice to meet you too.\n\n" +
            "Khi gặp ai đó lần đầu, hãy chào và giới thiệu tên của bạn.";

        public const string Numbers =
            "Numbers help you tell the time and count things.\n\n" +
            "A: What time is it?\n" +
            "B: It is seven o'clock.\n" +
            "A: And how many books do you have?\n" +
            "B: I have three books.";

        public const string Cafe =
            "At a cafe you order food and drinks politely.\n\n" +
            "Waiter: Good morning. What would you like?\n" +
            "You: A coffee, please.\n" +
            "Waiter: Sure. Here you are.\n" +
            "You: Thank you.";

        public const string Friends =
            "When you meet a friend, ask how they are.\n\n" +
            "A: Hi! How are you?\n" +
            "B: I'm good, thanks. And you?\n" +
            "A: Great. This is my friend Lan.";

        public const string Airport =
            "At the airport you show your ticket and find your gate.\n\n" +
            "Officer: Can I see your ticket, please?\n" +
            "You: Yes. Here it is.\n" +
            "Officer: Gate 12 is on the left.";

        public const string Directions =
            "When you are lost, ask for directions politely.\n\n" +
            "A: Excuse me, where is the station?\n" +
            "B: Go straight and turn left.\n" +
            "A: Thank you very much.";
    }

    internal static class LessonGrammar
    {
        public const string Hello =
            "Use Hello / Hi to greet someone. Use My name is + name to introduce yourself.";

        public const string Numbers =
            "Use It is + time to tell the time. Use How many + plural noun for counting.";

        public const string Cafe =
            "Use I'd like / A + drink, please to order politely. Add please to be polite.";

        public const string Friends =
            "Use How are you? and I'm + adjective to talk about feelings.";

        public const string Airport =
            "Use Can I + verb to make a polite request. Here it is = đây rồi.";

        public const string Directions =
            "Use Excuse me to start. Go straight / turn left / turn right for directions.";
    }
}
