using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Identity
{
    internal static class SeedCatalog
    {
        public static async Task EnrichAsync(ApplicationDbContext context, string webRoot)
        {
            SeedMedia.Ensure(webRoot);

            var lessons = await context.Lessons.ToListAsync();
            var byTitle = lessons.ToDictionary(x => x.Title, x => x, StringComparer.OrdinalIgnoreCase);

            void FillLesson(string title, string content, string grammar, string videoUrl)
            {
                if (!byTitle.TryGetValue(title, out var lesson))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(lesson.Content))
                {
                    lesson.Content = content;
                }

                if (string.IsNullOrWhiteSpace(lesson.GrammarNotes))
                {
                    lesson.GrammarNotes = grammar;
                }

                if (string.IsNullOrWhiteSpace(lesson.VideoUrl))
                {
                    lesson.VideoUrl = videoUrl;
                }
            }

            FillLesson("Hello and Introductions", LessonContents.Hello, LessonGrammar.Hello,
                "https://www.youtube.com/watch?v=gVfgkFaswn4");
            FillLesson("Numbers and Time", LessonContents.Numbers, LessonGrammar.Numbers,
                "https://www.youtube.com/watch?v=DR-cfDsHCGA");
            FillLesson("At the Cafe", LessonContents.Cafe, LessonGrammar.Cafe,
                "https://www.youtube.com/watch?v=KYkl5eYbcVY");
            FillLesson("Meeting Friends", LessonContents.Friends, LessonGrammar.Friends,
                "https://www.youtube.com/watch?v=oxNo1uvoc-I");
            FillLesson("At the Airport", LessonContents.Airport, LessonGrammar.Airport,
                "https://www.youtube.com/watch?v=l8e5CKs5z-g");
            FillLesson("Asking for Directions", LessonContents.Directions, LessonGrammar.Directions,
                "https://www.youtube.com/watch?v=kYXieg-sCsM");

            var courses = await context.courses.ToListAsync();
            foreach (var course in courses)
            {
                if (!string.IsNullOrWhiteSpace(course.ThumbnailUrl))
                {
                    continue;
                }

                course.ThumbnailUrl = course.Name switch
                {
                    "English for Beginners" => "/images/courses/english-for-beginners.svg",
                    "Daily Conversation" => "/images/courses/daily-conversation.svg",
                    "Travel English" => "/images/courses/travel-english.svg",
                    _ => course.ThumbnailUrl
                };
            }

            // Dữ liệu cũ từng lưu Type dạng có dấu cách ("Multiple Choice").
            var exercisesToNormalize = await context.exercises.ToListAsync();

            foreach (var exercise in exercisesToNormalize)
            {
                var normalized = ExerciseTypes.Normalize(exercise.Type);

                if (exercise.Type != normalized)
                {
                    exercise.Type = normalized;
                }
            }

            await context.SaveChangesAsync();

            int? LessonId(string title) =>
                byTitle.TryGetValue(title, out var lesson) ? lesson.Id : null;

            await EnsureWordAsync(context, LessonId("Hello and Introductions"), "hello", "/həˈləʊ/", "exclamation", "Used as a greeting", "xin chào", "Hello, my name is Anna.", "Xin chào, tôi tên là Anna.");
            await EnsureWordAsync(context, LessonId("Hello and Introductions"), "goodbye", "/ˌɡʊdˈbaɪ/", "exclamation", "Used when leaving", "tạm biệt", "Goodbye! See you tomorrow.", "Tạm biệt! Hẹn gặp lại ngày mai.");
            await EnsureWordAsync(context, LessonId("Hello and Introductions"), "name", "/neɪm/", "noun", "What a person is called", "tên", "What is your name?", "Bạn tên là gì?");
            await EnsureWordAsync(context, LessonId("At the Cafe"), "coffee", "/ˈkɒfi/", "noun", "A hot drink made from coffee beans", "cà phê", "I would like a cup of coffee.", "Tôi muốn một tách cà phê.");
            await EnsureWordAsync(context, LessonId("At the Cafe"), "please", "/pliːz/", "adverb", "Used to make a request polite", "làm ơn / vui lòng", "A coffee, please.", "Cho một ly cà phê, làm ơn.");
            await EnsureWordAsync(context, LessonId("At the Cafe"), "menu", "/ˈmenjuː/", "noun", "A list of food and drinks", "thực đơn", "Can I see the menu, please?", "Cho tôi xem thực đơn được không?");
            await EnsureWordAsync(context, LessonId("At the Cafe"), "water", "/ˈwɔːtə/", "noun", "A clear drink", "nước", "I would like some water.", "Tôi muốn một ít nước.");
            await EnsureWordAsync(context, LessonId("At the Airport"), "ticket", "/ˈtɪkɪt/", "noun", "A piece of paper that lets you travel", "vé", "Can I see your ticket, please?", "Cho tôi xem vé của bạn được không?");
            await EnsureWordAsync(context, LessonId("At the Airport"), "airport", "/ˈeəpɔːt/", "noun", "A place where airplanes take off and land", "sân bay", "We arrived at the airport early.", "Chúng tôi đến sân bay sớm.");
            await EnsureWordAsync(context, LessonId("At the Airport"), "gate", "/ɡeɪt/", "noun", "The door where you board a plane", "cổng lên máy bay", "Gate 12 is on the left.", "Cổng 12 ở bên trái.");
            await EnsureWordAsync(context, LessonId("At the Airport"), "passport", "/ˈpɑːspɔːt/", "noun", "An official document for travel", "hộ chiếu", "Here is my passport.", "Đây là hộ chiếu của tôi.");
            await EnsureWordAsync(context, LessonId("Numbers and Time"), "number", "/ˈnʌmbə/", "noun", "A symbol used for counting", "số", "What is your phone number?", "Số điện thoại của bạn là gì?");
            await EnsureWordAsync(context, LessonId("Numbers and Time"), "time", "/taɪm/", "noun", "Clock time in the day", "thời gian / giờ", "What time is it?", "Bây giờ là mấy giờ?");
            await EnsureWordAsync(context, LessonId("Numbers and Time"), "o'clock", "/əˈklɒk/", "adverb", "Used to say the exact hour", "giờ đúng", "It is seven o'clock.", "Bây giờ là bảy giờ.");
            await EnsureWordAsync(context, LessonId("Numbers and Time"), "today", "/təˈdeɪ/", "noun", "This day", "hôm nay", "Today is Monday.", "Hôm nay là thứ Hai.");
            await EnsureWordAsync(context, LessonId("Meeting Friends"), "friend", "/frend/", "noun", "A person you like and know well", "bạn bè", "She is my best friend.", "Cô ấy là bạn thân của tôi.");
            await EnsureWordAsync(context, LessonId("Meeting Friends"), "thanks", "/θæŋks/", "exclamation", "A short way to say thank you", "cảm ơn", "I'm good, thanks.", "Tôi khỏe, cảm ơn.");
            await EnsureWordAsync(context, LessonId("Meeting Friends"), "nice", "/naɪs/", "adjective", "Pleasant or kind", "tốt / dễ chịu", "Nice to meet you.", "Rất vui được gặp bạn.");
            await EnsureWordAsync(context, LessonId("Meeting Friends"), "meet", "/miːt/", "verb", "To see and speak to someone", "gặp", "Nice to meet you.", "Rất vui được gặp bạn.");
            await EnsureWordAsync(context, LessonId("Asking for Directions"), "excuse", "/ɪkˈskjuːz/", "verb", "Used politely to start talking", "xin lỗi / làm phiền", "Excuse me, where is the station?", "Xin lỗi, nhà ga ở đâu?");
            await EnsureWordAsync(context, LessonId("Asking for Directions"), "left", "/left/", "noun", "The opposite of right", "bên trái", "Turn left at the corner.", "Rẽ trái ở góc phố.");
            await EnsureWordAsync(context, LessonId("Asking for Directions"), "right", "/raɪt/", "noun", "The opposite of left", "bên phải", "The bank is on the right.", "Ngân hàng ở bên phải.");
            await EnsureWordAsync(context, LessonId("Asking for Directions"), "station", "/ˈsteɪʃn/", "noun", "A place for trains", "nhà ga", "Where is the station?", "Nhà ga ở đâu?");
            await EnsureWordAsync(context, LessonId("Asking for Directions"), "straight", "/streɪt/", "adverb", "Forward, not left or right", "đi thẳng", "Go straight and turn left.", "Đi thẳng rồi rẽ trái.");
            await EnsureWordAsync(context, LessonId("Asking for Directions"), "where", "/weə/", "adverb", "Used to ask about a place", "ở đâu", "Where is the station?", "Nhà ga ở đâu?");

            await EnsureListeningAsync(
                context,
                LessonId("Hello and Introductions"),
                "/audio/hello-introductions.mp3",
                SeedMedia.ListeningScripts["hello-introductions"],
                ("What is the girl's name?", "Anna"));
            await EnsureListeningAsync(
                context,
                LessonId("At the Cafe"),
                "/audio/at-the-cafe.mp3",
                SeedMedia.ListeningScripts["at-the-cafe"],
                ("What does the customer order?", "A coffee"));
            await EnsureListeningAsync(
                context,
                LessonId("At the Airport"),
                "/audio/at-the-airport.mp3",
                SeedMedia.ListeningScripts["at-the-airport"],
                ("Which gate is mentioned?", "Gate 12"));
            await EnsureListeningAsync(
                context,
                LessonId("Numbers and Time"),
                "/audio/numbers-and-time.mp3",
                SeedMedia.ListeningScripts["numbers-and-time"],
                ("What time is it?", "seven o'clock"),
                ("How many books does the speaker have?", "three"));
            await EnsureListeningAsync(
                context,
                LessonId("Meeting Friends"),
                "/audio/meeting-friends.mp3",
                SeedMedia.ListeningScripts["meeting-friends"],
                ("How is the friend?", "good"),
                ("What is the friend's name?", "Lan"));
            await EnsureListeningAsync(
                context,
                LessonId("Asking for Directions"),
                "/audio/asking-for-directions.mp3",
                SeedMedia.ListeningScripts["asking-for-directions"],
                ("Where does A want to go?", "the station"),
                ("How do you get there?", "Go straight and turn left"));

            await EnsureExerciseAsync(
                context,
                LessonId("Hello and Introductions"),
                "Chọn câu chào hỏi phù hợp",
                new[]
                {
                    ("What do you say when you meet someone?", "Hello", "Good night", "See you later",
                        "Hello is a common greeting."),
                    ("Which sentence introduces your name?", "My name is Lan.", "I am 20 years old.", "I live in Hanoi.",
                        "My name is... is used to introduce yourself.")
                });
            await EnsureExerciseAsync(
                context,
                LessonId("At the Cafe"),
                "Gọi món tại quán cà phê",
                new[]
                {
                    ("How do you politely order a drink?", "A coffee, please.", "Give coffee now.", "Coffee you.",
                        "Add please to make the request polite.")
                });
            await EnsureExerciseAsync(
                context,
                LessonId("At the Airport"),
                "Tình huống tại sân bay",
                new[]
                {
                    ("Where do airplanes take off and land?", "Airport", "Library", "Supermarket",
                        "An airport is the place for flights.")
                });
            await EnsureExerciseAsync(
                context,
                LessonId("Numbers and Time"),
                "Số đếm và hỏi giờ",
                new[]
                {
                    ("What time is it?", "It is seven o'clock.", "I am seven years old.", "It is Monday.",
                        "Use It is + time to tell the time."),
                    ("How many books do you have?", "I have three books.", "I have book.", "Yes, I am.",
                        "Use How many + plural noun.")
                });
            await EnsureExerciseAsync(
                context,
                LessonId("Meeting Friends"),
                "Gặp bạn bè",
                new[]
                {
                    ("What do you say when you meet a friend?", "Hi! How are you?", "Where is the station?", "A coffee, please.",
                        "Ask How are you? when you meet a friend."),
                    ("Which sentence introduces another person?", "This is my friend Lan.", "I have three books.", "Gate 12 is on the left.",
                        "Use This is + name to introduce someone.")
                });
            await EnsureExerciseAsync(
                context,
                LessonId("Asking for Directions"),
                "Hỏi đường",
                new[]
                {
                    ("How do you start asking for directions?", "Excuse me, where is the station?", "Hello, my name is Anna.", "I'm good, thanks.",
                        "Use Excuse me to start politely."),
                    ("Which instruction means 'đi thẳng rồi rẽ trái'?", "Go straight and turn left.", "Turn right and sit down.", "Gate 12 is on the left.",
                        "Go straight / turn left / turn right are direction phrases.")
                });

            await EnsureFillBlankExerciseAsync(
                context,
                LessonId("Hello and Introductions"),
                "Điền từ còn thiếu khi chào hỏi",
                new[]
                {
                    ("Hello! My ____ is Anna. (điền 1 từ)", "name",
                        "My name is + tên để giới thiệu bản thân."),
                    ("Nice to ____ you. (điền 1 từ)", "meet",
                        "Nice to meet you là câu chào khi gặp lần đầu.")
                });
            await EnsureFillBlankExerciseAsync(
                context,
                LessonId("At the Cafe"),
                "Điền từ khi gọi món",
                new[]
                {
                    ("A coffee, ____. (điền 1 từ cho lịch sự)", "please",
                        "Thêm please để câu yêu cầu lịch sự hơn."),
                    ("Can I see the ____, please? (danh sách món)", "menu",
                        "Menu là thực đơn của quán.")
                });

            await EnsureChoiceListeningQuestionAsync(
                context,
                LessonId("Hello and Introductions"),
                "Ai giới thiệu tên mình trước?",
                "Anna",
                "Anna|Minh|Lan");
            await EnsureChoiceListeningQuestionAsync(
                context,
                LessonId("At the Airport"),
                "Cổng lên máy bay ở phía nào?",
                "on the left",
                "on the left|on the right|straight ahead");
            await EnsureTypedListeningQuestionAsync(
                context,
                LessonId("Hello and Introductions"),
                ListeningQuestionTypes.TranscriptGap,
                "Hi! My name is ___. Nice to meet you.",
                "Anna");
            await EnsureTypedListeningQuestionAsync(
                context,
                LessonId("Hello and Introductions"),
                ListeningQuestionTypes.Sentence,
                "Nghe rồi gõ lại câu này.",
                "Nice to meet you.");
            await EnsureTypedListeningQuestionAsync(
                context,
                LessonId("At the Cafe"),
                ListeningQuestionTypes.Dictation,
                "Gõ lại đúng chính tả câu bạn nghe.",
                "A coffee, please.");
            await EnsureTypedListeningQuestionAsync(
                context,
                LessonId("Asking for Directions"),
                ListeningQuestionTypes.TranscriptGap,
                "Excuse me, where is the ___?",
                "station");
            await EnsureTypedListeningQuestionAsync(
                context,
                LessonId("Asking for Directions"),
                ListeningQuestionTypes.Sentence,
                "Nghe rồi gõ lại chỉ dẫn.",
                "Go straight and turn left.");

            await SeedSkillBank.EnsureAsync(context);
        }

        private static async Task EnsureFillBlankExerciseAsync(
            ApplicationDbContext context,
            int? lessonId,
            string title,
            (string Question, string Answer, string Explanation)[] items)
        {
            if (!lessonId.HasValue)
            {
                return;
            }

            if (await context.exercises.AnyAsync(x =>
                    x.LessonId == lessonId.Value &&
                    x.Type == ExerciseTypes.FillBlank))
            {
                return;
            }

            var nextOrder = await context.exercises
                .Where(x => x.LessonId == lessonId.Value)
                .CountAsync() + 1;

            var exercise = new Exercise
            {
                LessonId = lessonId.Value,
                Type = ExerciseTypes.FillBlank,
                Question = title,
                ExerciseOrder = nextOrder
            };
            context.exercises.Add(exercise);
            await context.SaveChangesAsync();

            foreach (var item in items)
            {
                var question = new Question
                {
                    ExerciseId = exercise.Id,
                    QuestionText = item.Question,
                    Explanation = item.Explanation
                };
                context.questions.Add(question);
                await context.SaveChangesAsync();

                context.answers.Add(new Answer
                {
                    QuestionId = question.Id,
                    AnswerText = item.Answer,
                    IsCorrect = true
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureChoiceListeningQuestionAsync(
            ApplicationDbContext context,
            int? lessonId,
            string question,
            string answer,
            string options)
        {
            if (!lessonId.HasValue)
            {
                return;
            }

            var listening = await context.ListeningLessons
                .FirstOrDefaultAsync(x => x.LessonId == lessonId.Value);

            if (listening == null)
            {
                return;
            }

            if (await context.ListeningQuestions.AnyAsync(x =>
                    x.ListeningLessonId == listening.Id &&
                    x.QuestionType == ListeningQuestionTypes.MultipleChoice))
            {
                return;
            }

            context.ListeningQuestions.Add(new ListeningQuestion
            {
                ListeningLessonId = listening.Id,
                Question = question,
                Answer = answer,
                QuestionType = ListeningQuestionTypes.MultipleChoice,
                Options = options
            });

            await context.SaveChangesAsync();
        }

        private static async Task EnsureTypedListeningQuestionAsync(
            ApplicationDbContext context,
            int? lessonId,
            string questionType,
            string question,
            string answer)
        {
            if (!lessonId.HasValue)
            {
                return;
            }

            var listening = await context.ListeningLessons
                .FirstOrDefaultAsync(x => x.LessonId == lessonId.Value);

            if (listening == null)
            {
                return;
            }

            if (await context.ListeningQuestions.AnyAsync(x =>
                    x.ListeningLessonId == listening.Id &&
                    x.QuestionType == questionType &&
                    x.Question == question))
            {
                return;
            }

            context.ListeningQuestions.Add(new ListeningQuestion
            {
                ListeningLessonId = listening.Id,
                Question = question,
                Answer = answer,
                QuestionType = questionType
            });

            await context.SaveChangesAsync();
        }

        public static async Task EnsureDailyStreaksAsync(
            ApplicationDbContext context,
            string userId)
        {
            if (await context.DailyStreaks.AnyAsync(x => x.ApplicationUserId == userId))
            {
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Now);

            // (số ngày trước hôm nay, số phút học, điểm nhận được).
            // Chuỗi 6 ngày liên tiếp tính đến hôm nay, nghỉ 2 ngày,
            // trước đó là chuỗi 4 ngày.
            var samples = new (int DaysAgo, int Minutes, int Points)[]
            {
                (0, 25, 30),
                (1, 40, 55),
                (2, 15, 20),
                (3, 35, 45),
                (4, 50, 70),
                (5, 20, 25),
                (8, 30, 40),
                (9, 45, 60),
                (10, 10, 15),
                (11, 60, 80)
            };

            foreach (var sample in samples)
            {
                context.DailyStreaks.Add(new DailyStreak
                {
                    ApplicationUserId = userId,
                    StudyDate = today.AddDays(-sample.DaysAgo),
                    MinutesStudied = sample.Minutes,
                    PointsEarned = sample.Points
                });
            }

            await context.SaveChangesAsync();
        }

        internal static async Task EnsureWordAsync(
            ApplicationDbContext context,
            int? lessonId,
            string wordText,
            string pronunciation,
            string partOfSpeech,
            string definition,
            string viMeaning,
            string exampleEn,
            string exampleVi)
        {
            var existing = await context.words
                .FirstOrDefaultAsync(x => x.WordText.ToLower() == wordText.ToLower());
            var audioUrl = SeedMedia.WordAudioUrl(wordText);

            if (existing == null)
            {
                existing = new Word
                {
                    WordText = wordText,
                    Pronunciation = pronunciation,
                    PartOfSpeech = partOfSpeech,
                    Definition = definition,
                    LessonId = lessonId,
                    AudioUrl = audioUrl
                };
                context.words.Add(existing);
                await context.SaveChangesAsync();

                context.wordMeanings.Add(new WordMeaning
                {
                    WordId = existing.Id,
                    Language = "vi",
                    Meaning = viMeaning
                });
                context.wordExamples.Add(new WordExample
                {
                    WordId = existing.Id,
                    EnglishSentence = exampleEn,
                    VietnameseMeaning = exampleVi
                });
                await context.SaveChangesAsync();
                return;
            }

            if (!existing.LessonId.HasValue && lessonId.HasValue)
            {
                existing.LessonId = lessonId;
            }

            if (string.IsNullOrWhiteSpace(existing.AudioUrl))
            {
                existing.AudioUrl = audioUrl;
            }

            await context.SaveChangesAsync();
        }

        internal static async Task EnsureListeningAsync(
            ApplicationDbContext context,
            int? lessonId,
            string audioUrl,
            string transcript,
            params (string Question, string Answer)[] questions)
        {
            if (!lessonId.HasValue)
            {
                return;
            }

            var existing = await context.ListeningLessons
                .FirstOrDefaultAsync(x => x.LessonId == lessonId.Value);

            if (existing == null)
            {
                existing = new ListeningLesson
                {
                    LessonId = lessonId.Value,
                    AudioUrl = audioUrl,
                    Transcript = transcript
                };
                context.ListeningLessons.Add(existing);
                await context.SaveChangesAsync();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(existing.AudioUrl))
                {
                    existing.AudioUrl = audioUrl;
                }

                if (string.IsNullOrWhiteSpace(existing.Transcript))
                {
                    existing.Transcript = transcript;
                }

                await context.SaveChangesAsync();
            }

            if (!await context.ListeningQuestions.AnyAsync(x => x.ListeningLessonId == existing.Id))
            {
                foreach (var question in questions)
                {
                    context.ListeningQuestions.Add(new ListeningQuestion
                    {
                        ListeningLessonId = existing.Id,
                        Question = question.Question,
                        Answer = question.Answer
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        internal static async Task EnsureExerciseAsync(
            ApplicationDbContext context,
            int? lessonId,
            string title,
            (string Question, string Correct, string Wrong1, string Wrong2, string Explanation)[] items)
        {
            if (!lessonId.HasValue)
            {
                return;
            }

            if (await context.exercises.AnyAsync(x => x.LessonId == lessonId.Value))
            {
                return;
            }

            var exercise = new Exercise
            {
                LessonId = lessonId.Value,
                Type = ExerciseTypes.MultipleChoice,
                Question = title,
                ExerciseOrder = 1
            };
            context.exercises.Add(exercise);
            await context.SaveChangesAsync();

            foreach (var item in items)
            {
                var question = new Question
                {
                    ExerciseId = exercise.Id,
                    QuestionText = item.Question,
                    Explanation = item.Explanation
                };
                context.questions.Add(question);
                await context.SaveChangesAsync();

                context.answers.AddRange(
                    new Answer { QuestionId = question.Id, AnswerText = item.Correct, IsCorrect = true },
                    new Answer { QuestionId = question.Id, AnswerText = item.Wrong1, IsCorrect = false },
                    new Answer { QuestionId = question.Id, AnswerText = item.Wrong2, IsCorrect = false });
            }

            await context.SaveChangesAsync();
        }
    }
}
