using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Identity
{
    internal static class SeedSkillBank
    {
        public static async Task EnsureAsync(ApplicationDbContext context)
        {
            await EnsureExtraCoursesAsync(context);
            await EnsureWorkplaceSkillsAsync(context);
            await EnsureReadingAsync(context);
            await EnsureGrammarAsync(context);
            await EnsureWritingAsync(context);
            await SeedCurriculum.EnsureAsync(context);
            await SeedProductExperience.EnsureAsync(context);
        }

        private static async Task EnsureExtraCoursesAsync(ApplicationDbContext context)
        {
            if (await context.courses.AnyAsync(x => x.Name == "Workplace English"))
            {
                return;
            }

            var workTopic = await context.topics.FirstOrDefaultAsync(x => x.Name == "Work");
            if (workTopic == null)
            {
                workTopic = new Topic { Name = "Work", Description = "Công sở và email" };
                context.topics.Add(workTopic);
                await context.SaveChangesAsync();
            }

            var now = DateTime.UtcNow;
            var workplace = new Course
            {
                Name = "Workplace English",
                Description = "Email, họp ngắn và nói chuyện với đồng nghiệp.",
                Level = "Intermediate",
                ThumbnailUrl = "/images/courses/daily-conversation.svg",
                IsPublished = true,
                CreatedAt = now
            };
            var advanced = new Course
            {
                Name = "Advanced Expressions",
                Description = "Cụm từ tự nhiên, nhấn mạnh và văn phong trang trọng.",
                Level = "Advanced",
                ThumbnailUrl = "/images/courses/travel-english.svg",
                IsPublished = true,
                CreatedAt = now
            };
            context.courses.AddRange(workplace, advanced);
            await context.SaveChangesAsync();

            context.Lessons.AddRange(
                new Lesson
                {
                    CourseId = workplace.Id,
                    TopicId = workTopic.Id,
                    Title = "Writing a short email",
                    Description = "Viết email ngắn xin nghỉ hoặc xác nhận họp.",
                    Content = "Hi Anna,\n\nCould we meet at 3 p.m. tomorrow to talk about the report? I am free after lunch.\n\nBest regards,\nMinh",
                    GrammarNotes = "Could we + verb để đề nghị lịch sự. Best regards để kết email.",
                    LessonOrder = 1,
                    DurationMinutes = 25,
                    IsPublished = true
                },
                new Lesson
                {
                    CourseId = workplace.Id,
                    TopicId = workTopic.Id,
                    Title = "In a short meeting",
                    Description = "Mở họp, nêu ý kiến và kết thúc.",
                    Content = "Let's start. First, the sales numbers are up. I think we should call the client today. Does anyone have a question?",
                    GrammarNotes = "Let's + verb để đề nghị. I think we should + verb để góp ý.",
                    LessonOrder = 2,
                    DurationMinutes = 25,
                    IsPublished = true
                },
                new Lesson
                {
                    CourseId = advanced.Id,
                    TopicId = workTopic.Id,
                    Title = "Emphasis and contrast",
                    Description = "Nhấn mạnh ý và nêu sự tương phản.",
                    Content = "What I need is a clear plan. Although the idea is interesting, it is not practical yet. On the other hand, the budget is ready.",
                    GrammarNotes = "What I need is... để nhấn mạnh. Although / On the other hand để tương phản.",
                    LessonOrder = 1,
                    DurationMinutes = 30,
                    IsPublished = true
                });
            await context.SaveChangesAsync();
        }

        private static async Task EnsureWorkplaceSkillsAsync(ApplicationDbContext context)
        {
            var lessons = await context.Lessons.ToListAsync();
            int? Id(string title) =>
                lessons.FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase))?.Id;

            await SeedCatalog.EnsureWordAsync(context, Id("Writing a short email"), "email", "/ˈiːmeɪl/", "noun", "A message sent on a computer", "thư điện tử", "I sent an email to my manager.", "Tôi đã gửi email cho quản lý.");
            await SeedCatalog.EnsureWordAsync(context, Id("Writing a short email"), "regards", "/rɪˈɡɑːdz/", "noun", "A polite way to end a letter", "thân ái / trân trọng", "Best regards, Minh", "Trân trọng, Minh");
            await SeedCatalog.EnsureWordAsync(context, Id("Writing a short email"), "report", "/rɪˈpɔːt/", "noun", "A written description of work", "báo cáo", "Let's talk about the report.", "Chúng ta hãy nói về bản báo cáo.");
            await SeedCatalog.EnsureWordAsync(context, Id("Writing a short email"), "tomorrow", "/təˈmɒrəʊ/", "adverb", "The day after today", "ngày mai", "Could we meet tomorrow?", "Chúng ta họp ngày mai được không?");
            await SeedCatalog.EnsureWordAsync(context, Id("In a short meeting"), "meeting", "/ˈmiːtɪŋ/", "noun", "A time when people talk about work", "cuộc họp", "Let's start the meeting.", "Chúng ta bắt đầu cuộc họp.");
            await SeedCatalog.EnsureWordAsync(context, Id("In a short meeting"), "client", "/ˈklaɪənt/", "noun", "A person or company you work for", "khách hàng", "We should call the client today.", "Chúng ta nên gọi khách hàng hôm nay.");
            await SeedCatalog.EnsureWordAsync(context, Id("In a short meeting"), "sales", "/seɪlz/", "noun", "The number of things sold", "doanh số", "The sales numbers are up.", "Doanh số đang tăng.");
            await SeedCatalog.EnsureWordAsync(context, Id("In a short meeting"), "question", "/ˈkwestʃən/", "noun", "Something you ask", "câu hỏi", "Does anyone have a question?", "Ai có câu hỏi không?");
            await SeedCatalog.EnsureWordAsync(context, Id("Emphasis and contrast"), "although", "/ɔːlˈðəʊ/", "conjunction", "Used to show contrast", "mặc dù", "Although the idea is interesting, it is not practical yet.", "Mặc dù ý tưởng hay, nó chưa thực tế.");
            await SeedCatalog.EnsureWordAsync(context, Id("Emphasis and contrast"), "however", "/haʊˈevə/", "adverb", "Used to introduce a different idea", "tuy nhiên", "The plan is clear. However, we need more time.", "Kế hoạch rõ. Tuy nhiên chúng ta cần thêm thời gian.");
            await SeedCatalog.EnsureWordAsync(context, Id("Emphasis and contrast"), "practical", "/ˈpræktɪkl/", "adjective", "Useful in real life", "thực tế", "The idea is not practical yet.", "Ý tưởng chưa thực tế.");
            await SeedCatalog.EnsureWordAsync(context, Id("Emphasis and contrast"), "budget", "/ˈbʌdʒɪt/", "noun", "A plan for money", "ngân sách", "On the other hand, the budget is ready.", "Mặt khác, ngân sách đã sẵn sàng.");

            await SeedCatalog.EnsureListeningAsync(
                context,
                Id("Writing a short email"),
                "/audio/workplace-email.mp3",
                SeedMedia.ListeningScripts["workplace-email"],
                ("When do they want to meet?", "tomorrow"),
                ("What will they talk about?", "the report"));
            await SeedCatalog.EnsureListeningAsync(
                context,
                Id("In a short meeting"),
                "/audio/workplace-meeting.mp3",
                SeedMedia.ListeningScripts["workplace-meeting"],
                ("Are sales up or down?", "up"),
                ("Who should they call?", "the client"));
            await SeedCatalog.EnsureListeningAsync(
                context,
                Id("Emphasis and contrast"),
                "/audio/advanced-emphasis.mp3",
                SeedMedia.ListeningScripts["advanced-emphasis"],
                ("What does the speaker need?", "a clear plan"),
                ("Is the idea practical yet?", "no"));

            await SeedCatalog.EnsureExerciseAsync(
                context,
                Id("Writing a short email"),
                "Email công sở",
                new[]
                {
                    ("How do you end a formal email?", "Best regards", "See you", "Bye bye",
                        "Best regards is a polite closing."),
                    ("Which sentence asks for a meeting politely?", "Could we meet tomorrow?", "Meet me now.", "You come.",
                        "Could we + verb is polite.")
                });
            await SeedCatalog.EnsureExerciseAsync(
                context,
                Id("In a short meeting"),
                "Trong cuộc họp ngắn",
                new[]
                {
                    ("How do you start a meeting?", "Let's start.", "I go home.", "Best regards.",
                        "Let's + verb suggests an action."),
                    ("How do you invite questions?", "Does anyone have a question?", "Be quiet.", "Call the client.",
                        "Does anyone have a question? opens the floor.")
                });
            await SeedCatalog.EnsureExerciseAsync(
                context,
                Id("Emphasis and contrast"),
                "Nhấn mạnh và tương phản",
                new[]
                {
                    ("Which word introduces contrast?", "Although", "And", "Because",
                        "Although shows contrast."),
                    ("What I need is a clear plan emphasizes:", "the plan", "the budget", "the client",
                        "What I need is... highlights the noun after is.")
                });
        }

        private static async Task EnsureReadingAsync(ApplicationDbContext context)
        {
            if (await context.ReadingPassages.AnyAsync())
            {
                return;
            }

            var beginner = new ReadingPassage
            {
                Title = "A morning at the cafe",
                Level = "Beginner",
                Topic = "Daily Life",
                Body = "Lan goes to a small cafe every morning. She usually sits by the window and orders a cup of coffee. Today the cafe is busy. A man at the next table is reading a newspaper. Lan smiles and says hello. She has twenty minutes before work, so she does not stay long."
            };
            var elementary = new ReadingPassage
            {
                Title = "Missed the bus",
                Level = "Elementary",
                Topic = "Daily Life",
                Body = "Yesterday Minh woke up late. He ran to the bus stop, but the bus had already left. He decided to walk to the station. On the way he met his neighbor, Mrs Hoa, who offered him a ride. Minh arrived at work only ten minutes late. He promised himself to set two alarms next time."
            };
            var intermediate = new ReadingPassage
            {
                Title = "A new airport terminal",
                Level = "Intermediate",
                Topic = "Travel",
                Body = "The city opened a new airport terminal last month. Officials say it will reduce waiting time at security. Some passengers, however, still complain about long walks to the gates. The airport has not published the average walking time. Shop owners inside the terminal report higher sales, especially at coffee stands near Gate 12."
            };
            var advanced = new ReadingPassage
            {
                Title = "Why people keep a streak",
                Level = "Advanced",
                Topic = "Learning",
                Body = "Daily streaks are popular in learning apps because they turn effort into a visible chain. Researchers note that streaks can support habit building, yet they may also create anxiety when a learner is busy. The article does not claim that streaks improve test scores. It only suggests that learners who choose a realistic daily goal are more likely to return the next day."
            };

            context.ReadingPassages.AddRange(beginner, elementary, intermediate, advanced);
            await context.SaveChangesAsync();

            context.ReadingQuestions.AddRange(
                Q(beginner.Id, "MainIdea", "Đoạn văn nói về điều gì?",
                    "Lan uống cà phê trước khi đi làm|Lan mở một quán cà phê|Lan đọc báo mỗi sáng|Lan nghỉ việc",
                    "Lan uống cà phê trước khi đi làm",
                    "Cả đoạn kể thói quen buổi sáng của Lan ở quán cà phê."),
                Q(beginner.Id, "Detail", "Lan thường ngồi ở đâu?",
                    "By the window|At the counter|Outside|Near the kitchen",
                    "By the window",
                    "Câu 2: She usually sits by the window."),
                Q(beginner.Id, "TrueFalseNotGiven", "Lan order trà.",
                    "True|False|Not Given",
                    "False",
                    "Bài viết cô order coffee, không phải trà."),
                Q(beginner.Id, "Inference", "Vì sao Lan không ở lại lâu?",
                    "Cô phải đi làm|Quán hết chỗ|Cô không thích cà phê|Người đàn ông nói chuyện quá nhiều",
                    "Cô phải đi làm",
                    "She has twenty minutes before work."),
                Q(elementary.Id, "MainIdea", "Câu nào tóm ý chính?",
                    "Minh đi muộn nhưng vẫn đến được chỗ làm|Minh mua xe buýt mới|Minh nghỉ làm|Minh quên chìa khóa",
                    "Minh đi muộn nhưng vẫn đến được chỗ làm",
                    "Toàn bộ kể việc Minh lỡ xe buýt và vẫn tới công ty."),
                Q(elementary.Id, "Detail", "Ai cho Minh đi nhờ?",
                    "Mrs Hoa|His boss|A bus driver|Anna",
                    "Mrs Hoa",
                    "His neighbor, Mrs Hoa, offered him a ride."),
                Q(elementary.Id, "TrueFalseNotGiven", "Minh đến muộn 30 phút.",
                    "True|False|Not Given",
                    "False",
                    "Bài nói only ten minutes late."),
                Q(elementary.Id, "Inference", "Minh sẽ làm gì lần sau?",
                    "Đặt hai báo thức|Mua xe|Chuyển nhà|Nghỉ việc",
                    "Đặt hai báo thức",
                    "He promised to set two alarms next time."),
                Q(intermediate.Id, "MainIdea", "Bài viết chủ yếu bàn về?",
                    "Nhà ga sân bay mới và phản ứng của hành khách|Cách đặt vé máy bay|Lịch sử thành phố|Thời tiết mùa mưa",
                    "Nhà ga sân bay mới và phản ứng của hành khách",
                    "Mở đầu và các câu sau đều xoay quanh terminal mới."),
                Q(intermediate.Id, "Detail", "Cửa hàng nào bán chạy?",
                    "Coffee stands near Gate 12|Bookshops|Clothes stores|Pharmacies",
                    "Coffee stands near Gate 12",
                    "especially at coffee stands near Gate 12."),
                Q(intermediate.Id, "TrueFalseNotGiven", "Thời gian đi bộ trung bình đã được công bố.",
                    "True|False|Not Given",
                    "False",
                    "The airport has not published the average walking time."),
                Q(intermediate.Id, "Inference", "Một số hành khách không hài lòng vì?",
                    "Phải đi bộ xa tới cổng|Vé đắt hơn|Mất hành lý|Nhân viên thô lỗ",
                    "Phải đi bộ xa tới cổng",
                    "They complain about long walks to the gates."),
                Q(advanced.Id, "MainIdea", "Tác giả muốn người đọc hiểu điều gì?",
                    "Streak hữu ích nhưng cũng có thể gây áp lực|Streak luôn làm tăng điểm thi|Không nên học mỗi ngày|App học không cần mục tiêu",
                    "Streak hữu ích nhưng cũng có thể gây áp lực",
                    "Đoạn nêu lợi ích và lo âu, không khẳng định tăng điểm."),
                Q(advanced.Id, "TrueFalseNotGiven", "Streak chắc chắn cải thiện điểm kiểm tra.",
                    "True|False|Not Given",
                    "False",
                    "The article does not claim that streaks improve test scores."),
                Q(advanced.Id, "Inference", "Ai có khả năng học lại ngày hôm sau hơn?",
                    "Người chọn mục tiêu ngày thực tế|Người đặt streak 365 ngày ngay từ đầu|Người không mở app|Người chỉ học cuối tuần",
                    "Người chọn mục tiêu ngày thực tế",
                    "learners who choose a realistic daily goal are more likely to return."));

            await context.SaveChangesAsync();
        }

        private static ReadingQuestion Q(
            int passageId,
            string type,
            string prompt,
            string options,
            string answer,
            string explanation)
        {
            return new ReadingQuestion
            {
                ReadingPassageId = passageId,
                Type = type,
                Prompt = prompt,
                Options = options,
                Answer = answer,
                Explanation = explanation
            };
        }

        private static async Task EnsureGrammarAsync(ApplicationDbContext context)
        {
            if (await context.GrammarTopics.AnyAsync())
            {
                return;
            }

            var be = Topic("Present simple of be", "Beginner", 1,
                "Dùng am / is / are để nói tên, nghề, cảm xúc và vị trí.\nI am, you are, he/she/it is, we/they are.\nPhủ định: I am not, she is not (isn't), they are not (aren't).\nCâu hỏi: Are you a student? Is she at home?",
                "I am a student.\nShe is tired.\nAre they at the cafe?");
            var present = Topic("Present simple", "Beginner", 2,
                "Dùng hiện tại đơn cho thói quen và sự thật.\nI/you/we/they + V: I work.\nHe/she/it + V-s: She works.\nPhủ định: do not / does not + V.\nCâu hỏi: Do you live here? Does he like coffee?",
                "I go to work at 8.\nShe likes tea.\nDoes he speak English?");
            var articles = Topic("A / an / the", "Elementary", 3,
                "a + phụ âm: a book, a cafe.\nan + nguyên âm: an apple, an hour.\nthe khi người nghe đã biết vật đó: the menu, the station.\nKhông dùng mạo từ với plural chung: Books are useful.",
                "I need a ticket.\nShe wants an orange.\nOpen the window.");
            var past = Topic("Past simple", "Elementary", 4,
                "Hành động đã kết thúc. Động từ có quy tắc + ed: worked, needed.\nBất quy tắc: go → went, see → saw, have → had.\nPhủ định: did not + V nguyên mẫu.\nCâu hỏi: Did you see her yesterday?",
                "I arrived early.\nShe went home.\nDid they meet Lan?");
            var preps = Topic("Prepositions of place", "Intermediate", 5,
                "in: trong một không gian (in the cafe).\non: trên bề mặt / đường (on the left, on the table).\nat: điểm cụ thể (at the airport, at 7 o'clock).\nbetween / next to / opposite cho vị trí tương đối.",
                "The bank is on the right.\nWe met at the station.\nThe keys are on the table.");
            var cond = Topic("First conditional", "Intermediate", 6,
                "If + present simple, will + V: điều kiện có thể xảy ra.\nIf it rains, we will take a taxi.\nCó thể đảo: We will take a taxi if it rains.\nDon't dùng will trong mệnh đề if.",
                "If I have time, I will call you.\nIf she is late, we will start.");
            var relative = Topic("Relative clauses", "Advanced", 7,
                "who cho người, which cho vật, that cho cả hai trong mệnh đề xác định.\nThe man who called is my boss.\nThe report which you sent is clear.\nBỏ đại từ khi nó là tân ngữ: The book I bought is useful.",
                "The friend who helped me is Lan.\nThe gate that we need is on the left.");

            context.GrammarTopics.AddRange(be, present, articles, past, preps, cond, relative);
            await context.SaveChangesAsync();

            context.GrammarItems.AddRange(
                Item(be.Id, "I ___ a teacher.", "am|is|are|be", "am", "I đi với am."),
                Item(be.Id, "She ___ at the airport.", "am|is|are|be", "is", "She đi với is."),
                Item(be.Id, "___ they students?", "Is|Are|Am|Do", "Are", "They đi với are."),
                Item(present.Id, "He ___ coffee every morning.", "drink|drinks|drinking|drank", "drinks", "He/she/it thêm -s."),
                Item(present.Id, "They ___ in Hanoi.", "live|lives|living|lived", "live", "They + động từ nguyên mẫu."),
                Item(present.Id, "___ she work here?", "Do|Does|Is|Are", "Does", "Does + he/she/it."),
                Item(articles.Id, "I would like ___ apple.", "a|an|the|—", "an", "apple bắt đầu bằng nguyên âm."),
                Item(articles.Id, "Please open ___ window. (cửa sổ ta đang nói tới)", "a|an|the|—", "the", "Đã xác định cửa sổ nào."),
                Item(articles.Id, "She bought ___ ticket.", "a|an|the|—", "a", "ticket bắt đầu bằng phụ âm."),
                Item(past.Id, "Yesterday we ___ to the station.", "go|goes|went|gone", "went", "go bất quy tắc → went."),
                Item(past.Id, "He ___ not see the bus.", "did|does|do|was", "did", "Phủ định quá khứ: did not + V."),
                Item(past.Id, "___ you meet Lan last night?", "Do|Did|Are|Have", "Did", "Câu hỏi quá khứ dùng Did."),
                Item(preps.Id, "We arrived ___ the airport early.", "in|on|at|to", "at", "at the airport là điểm cụ thể."),
                Item(preps.Id, "The bank is ___ the left.", "in|on|at|between", "on", "on the left / on the right."),
                Item(preps.Id, "The keys are ___ the table.", "in|on|at|between", "on", "on = trên bề mặt."),
                Item(cond.Id, "If it rains, we ___ a taxi.", "take|takes|will take|took", "will take", "Mệnh đề chính dùng will."),
                Item(cond.Id, "If she ___ late, we will start.", "is|will be|was|be", "is", "Mệnh đề if dùng hiện tại."),
                Item(cond.Id, "We will call you if we ___ time.", "have|will have|had|has", "have", "If + present simple."),
                Item(relative.Id, "The woman ___ called you is my manager.", "which|who|where|when", "who", "who cho người."),
                Item(relative.Id, "The report ___ you sent is clear.", "who|where|which|when", "which", "which cho vật."),
                Item(relative.Id, "This is the gate ___ we need.", "who|when|that|whose", "that", "that được dùng cho vật trong mệnh đề xác định."));

            await context.SaveChangesAsync();
        }

        private static GrammarTopic Topic(string title, string level, int order, string explanation, string examples)
        {
            return new GrammarTopic
            {
                Title = title,
                Level = level,
                SortOrder = order,
                Explanation = explanation,
                Examples = examples
            };
        }

        private static GrammarItem Item(int topicId, string question, string options, string answer, string explanation)
        {
            return new GrammarItem
            {
                GrammarTopicId = topicId,
                QuestionText = question,
                Options = options,
                Answer = answer,
                Explanation = explanation
            };
        }

        private static async Task EnsureWritingAsync(ApplicationDbContext context)
        {
            if (await context.WritingPrompts.AnyAsync())
            {
                return;
            }

            context.WritingPrompts.AddRange(
                new WritingPrompt
                {
                    Title = "Introduce yourself",
                    Level = "Beginner",
                    Prompt = "Viết 4–6 câu giới thiệu tên, nghề hoặc lớp, và sở thích.",
                    KeyPoints = "name|student|like|I",
                    SampleAnswer = "My name is Lan. I am a student. I live in Hanoi. I like coffee and English.",
                    MinWords = 20
                },
                new WritingPrompt
                {
                    Title = "A problem on the way",
                    Level = "Elementary",
                    Prompt = "Kể một lần bạn đi muộn. Có chuyện gì và bạn đã làm gì?",
                    KeyPoints = "late|bus|walk|work",
                    SampleAnswer = "Yesterday I was late. I missed the bus, so I walked to the station. I arrived at work ten minutes late.",
                    MinWords = 30
                },
                new WritingPrompt
                {
                    Title = "A short work email",
                    Level = "Intermediate",
                    Prompt = "Viết email ngắn đề nghị họp vào ngày mai và kết bằng lời chào lịch sự.",
                    KeyPoints = "meet|tomorrow|regards|could",
                    SampleAnswer = "Hi Anna, could we meet tomorrow afternoon to talk about the report? I am free after lunch. Best regards, Minh",
                    MinWords = 35
                },
                new WritingPrompt
                {
                    Title = "Opinion on daily streaks",
                    Level = "Advanced",
                    Prompt = "Bạn nghĩ chuỗi ngày học có giúp hay gây áp lực? Nêu 2 lý do.",
                    KeyPoints = "streak|habit|goal|however",
                    SampleAnswer = "A daily streak can help a habit if the goal is realistic. However, it may create anxiety when people are busy. I prefer a small daily goal.",
                    MinWords = 45
                });

            await context.SaveChangesAsync();
        }
    }
}
