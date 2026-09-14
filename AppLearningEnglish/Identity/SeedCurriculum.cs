using AppLearningEnglish.Business;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Identity
{
    internal static class SeedCurriculum
    {
        public static async Task EnsureAsync(ApplicationDbContext context)
        {
            await EnsureExtraLessonsAsync(context);
            await EnsureMorePassagesAsync(context);
            await EnsureMoreGrammarAsync(context);
            await EnsureMoreWritingAsync(context);
            await EnsurePlacementAsync(context);
            await EnsureNewLessonSkillsAsync(context);
            await LinkLessonsAsync(context);
        }

        private static async Task EnsureExtraLessonsAsync(ApplicationDbContext context)
        {
            var now = DateTime.UtcNow;
            var daily = await context.topics.FirstOrDefaultAsync(x => x.Name == "Daily Life");
            var travel = await context.topics.FirstOrDefaultAsync(x => x.Name == "Travel");
            var food = await context.topics.FirstOrDefaultAsync(x => x.Name == "Food");
            if (daily == null || travel == null || food == null)
            {
                return;
            }

            if (!await context.courses.AnyAsync(x => x.Name == "Shopping English"))
            {
                context.courses.Add(new Course
                {
                    Name = "Shopping English",
                    Description = "Mua sắm, hỏi giá và đổi size.",
                    Level = "Elementary",
                    ThumbnailUrl = "/images/courses/daily-conversation.svg",
                    IsPublished = true,
                    CreatedAt = now
                });
                await context.SaveChangesAsync();
            }

            var beginner = await context.courses.FirstAsync(x => x.Name == "English for Beginners");
            var conversation = await context.courses.FirstAsync(x => x.Name == "Daily Conversation");
            var travelCourse = await context.courses.FirstAsync(x => x.Name == "Travel English");
            var shopping = await context.courses.FirstAsync(x => x.Name == "Shopping English");

            await AddLessonAsync(context, beginner.Id, daily.Id, 3, "My family",
                "Nói về người thân.",
                "This is my family. My mother is a teacher. My father works in a shop. I have one brother.",
                "This is + noun. Have / has để nói sở hữu.");
            await AddLessonAsync(context, conversation.Id, daily.Id, 3, "Making plans",
                "Rủ bạn đi chơi.",
                "Are you free on Saturday? Let's go to the cafe at 4 o'clock. I can meet you there.",
                "Are you free + day. Let's + verb. Can để đề nghị.");
            await AddLessonAsync(context, travelCourse.Id, travel.Id, 3, "At the hotel",
                "Nhận phòng khách sạn.",
                "I have a reservation. My name is Minh. Can I have the key, please? The room is on the second floor.",
                "Have a reservation. Can I have + noun.");
            await AddLessonAsync(context, shopping.Id, food.Id, 1, "At the shop",
                "Hỏi mua đồ.",
                "Excuse me, do you have this in a smaller size? How much is it? I'll take it.",
                "Do you have... How much is it? I'll take it.");
            await AddLessonAsync(context, shopping.Id, food.Id, 2, "Asking the price",
                "Hỏi giá và trả tiền.",
                "How much are these apples? They are two dollars. Can I pay by card?",
                "How much is / are. Pay by card / cash.");
        }

        private static async Task AddLessonAsync(
            ApplicationDbContext context,
            int courseId,
            int topicId,
            int order,
            string title,
            string description,
            string content,
            string grammar)
        {
            if (await context.Lessons.AnyAsync(x => x.Title == title))
            {
                return;
            }

            context.Lessons.Add(new Lesson
            {
                CourseId = courseId,
                TopicId = topicId,
                Title = title,
                Description = description,
                Content = content,
                GrammarNotes = grammar,
                LessonOrder = order,
                DurationMinutes = 25,
                IsPublished = true
            });
            await context.SaveChangesAsync();
        }

        private static async Task EnsureMorePassagesAsync(ApplicationDbContext context)
        {
            await AddPassageAsync(context, "A day with my family", "Beginner", "Daily Life",
                "On Sunday I stay at home with my family. My mother cooks lunch. My brother and I help. In the afternoon we watch a film. I like Sundays because we talk and eat together.",
                ("MainIdea", "Đoạn nói về điều gì?", "Một ngày chủ nhật với gia đình|Một chuyến đi|Một buổi họp|Một lớp học", "Một ngày chủ nhật với gia đình", "Cả đoạn kể Sunday ở nhà."),
                ("Detail", "Ai nấu ăn?", "My mother|My father|My brother|A neighbor", "My mother", "My mother cooks lunch."));
            await AddPassageAsync(context, "Saturday plans", "Elementary", "Daily Life",
                "Lan texts Minh. She is free on Saturday. They want to meet at the cafe at four. Minh is a little late, but they still have time for coffee and a short walk.",
                ("MainIdea", "Hai người định làm gì?", "Gặp nhau uống cà phê|Bay máy bay|Họp công ty|Thi cử", "Gặp nhau uống cà phê", "Meet at the cafe."),
                ("TrueFalseNotGiven", "Minh đến sớm.", "True|False|Not Given", "False", "Minh is a little late."));
            await AddPassageAsync(context, "Checking in at a hotel", "Intermediate", "Travel",
                "Minh arrives at the hotel after a long flight. He has a reservation. The receptionist gives him a key and says the room is on the second floor. Breakfast starts at seven.",
                ("MainIdea", "Minh đang làm gì?", "Nhận phòng khách sạn|Mua vé|Hỏi đường|Gọi món", "Nhận phòng khách sạn", "Reservation and key."),
                ("Detail", "Bữa sáng bắt đầu lúc nào?", "seven|four|twelve|nine", "seven", "Breakfast starts at seven."));
            await AddPassageAsync(context, "Buying a shirt", "Elementary", "Shopping",
                "Anna wants a blue shirt. The first one is too big. The shop assistant brings a smaller size. It costs 15 dollars. Anna pays by card.",
                ("MainIdea", "Anna làm gì?", "Mua áo|Bay|Nấu ăn|Dạy học", "Mua áo", "Buying a shirt."),
                ("Detail", "Áo giá bao nhiêu?", "15 dollars|3 dollars|50 dollars|2 dollars", "15 dollars", "It costs 15 dollars."));
        }

        private static async Task AddPassageAsync(
            ApplicationDbContext context,
            string title,
            string level,
            string topic,
            string body,
            params (string Type, string Prompt, string Options, string Answer, string Explanation)[] questions)
        {
            if (await context.ReadingPassages.AnyAsync(x => x.Title == title))
            {
                return;
            }

            var passage = new ReadingPassage
            {
                Title = title,
                Level = level,
                Topic = topic,
                Body = body
            };
            context.ReadingPassages.Add(passage);
            await context.SaveChangesAsync();

            foreach (var q in questions)
            {
                context.ReadingQuestions.Add(new ReadingQuestion
                {
                    ReadingPassageId = passage.Id,
                    Type = q.Type,
                    Prompt = q.Prompt,
                    Options = q.Options,
                    Answer = q.Answer,
                    Explanation = q.Explanation
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureMoreGrammarAsync(ApplicationDbContext context)
        {
            await AddGrammarAsync(context, "Present continuous", "Beginner", 8,
                "Hành động đang xảy ra: am/is/are + V-ing.\nI am cooking. She is watching a film.\nCâu hỏi: Are you working?",
                "I am writing an email.\nThey are waiting.",
                ("They ___ watching a film.", "is|are|am|be", "are", "They + are."),
                ("I ___ cooking now.", "am|is|are|be", "am", "I + am + V-ing."));
            await AddGrammarAsync(context, "Have / has", "Beginner", 9,
                "Have/has nói sở hữu.\nI/you/we/they have. He/she/it has.\nI have one brother. She has a reservation.",
                "I have a ticket.\nHe has a key.",
                ("She ___ a reservation.", "have|has|having|had", "has", "She + has."),
                ("I ___ one brother.", "have|has|is|are", "have", "I + have."));
            await AddGrammarAsync(context, "Can for requests", "Elementary", 10,
                "Can I + verb để xin phép hoặc đề nghị lịch sự.\nCan I have the key, please?\nCan I pay by card?",
                "Can I see the menu?\nCan we meet tomorrow?",
                ("___ I have the key, please?", "Can|Does|Is|Are", "Can", "Can I + verb."),
                ("Can I ___ by card?", "pay|pays|paid|paying", "pay", "Can + động từ nguyên mẫu."));
        }

        private static async Task AddGrammarAsync(
            ApplicationDbContext context,
            string title,
            string level,
            int order,
            string explanation,
            string examples,
            params (string Question, string Options, string Answer, string Note)[] items)
        {
            if (await context.GrammarTopics.AnyAsync(x => x.Title == title))
            {
                return;
            }

            var topic = new GrammarTopic
            {
                Title = title,
                Level = level,
                SortOrder = order,
                Explanation = explanation,
                Examples = examples
            };
            context.GrammarTopics.Add(topic);
            await context.SaveChangesAsync();

            foreach (var item in items)
            {
                context.GrammarItems.Add(new GrammarItem
                {
                    GrammarTopicId = topic.Id,
                    QuestionText = item.Question,
                    Options = item.Options,
                    Answer = item.Answer,
                    Explanation = item.Note
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureMoreWritingAsync(ApplicationDbContext context)
        {
            await AddPromptAsync(context, "My family", "Beginner",
                "Viết 4 câu về gia đình bạn.", "family|mother|father|have",
                "This is my family. My mother is a teacher. I have one brother. We live in Hanoi.", 20);
            await AddPromptAsync(context, "Weekend plan", "Elementary",
                "Rủ một người bạn gặp vào cuối tuần.", "free|Saturday|cafe|let's",
                "Are you free on Saturday? Let's go to the cafe at 4 o'clock.", 25);
            await AddPromptAsync(context, "Hotel check-in", "Intermediate",
                "Viết 4 câu khi nhận phòng.", "reservation|name|key|floor",
                "I have a reservation. My name is Minh. Can I have the key, please? The room is on the second floor.", 30);
            await AddPromptAsync(context, "At the shop", "Elementary",
                "Hỏi size và giá một món đồ.", "size|how much|take|please",
                "Excuse me, do you have this in a smaller size? How much is it? I'll take it.", 25);
        }

        private static async Task AddPromptAsync(
            ApplicationDbContext context,
            string title,
            string level,
            string prompt,
            string keyPoints,
            string sample,
            int minWords)
        {
            if (await context.WritingPrompts.AnyAsync(x => x.Title == title))
            {
                return;
            }

            context.WritingPrompts.Add(new WritingPrompt
            {
                Title = title,
                Level = level,
                Prompt = prompt,
                KeyPoints = keyPoints,
                SampleAnswer = sample,
                MinWords = minWords
            });
            await context.SaveChangesAsync();
        }

        private static async Task EnsurePlacementAsync(ApplicationDbContext context)
        {
            if (await context.PlacementItems.AnyAsync())
            {
                return;
            }

            var extra = new (string Text, string Options, string Answer)[]
            {
                ("I ___ cooking now.", "am|is|are|be", "am"),
                ("She ___ a reservation.", "have|has|having|is", "has"),
                ("___ I pay by card?", "Can|Does|Are|Is", "Can"),
                ("How much ___ these apples?", "is|are|am|be", "are"),
                ("We ___ meet you at the cafe.", "can|cans|are can|to", "can"),
                ("If I had time, I ___ call you.", "will|would|would to|can", "would"),
                ("The room ___ cleaned every day.", "is|are|have|has", "is"),
                ("I look forward to ___ you.", "meet|meeting|met|meets", "meeting")
            };

            var order = 1;
            foreach (var question in PlacementBank.Questions)
            {
                context.PlacementItems.Add(new PlacementItem
                {
                    Text = question.Text,
                    Options = string.Join("|", question.Options),
                    Answer = question.Answer,
                    SortOrder = order++
                });
            }

            foreach (var item in extra)
            {
                context.PlacementItems.Add(new PlacementItem
                {
                    Text = item.Text,
                    Options = item.Options,
                    Answer = item.Answer,
                    SortOrder = order++
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureNewLessonSkillsAsync(ApplicationDbContext context)
        {
            var lessons = await context.Lessons.ToListAsync();
            int? Id(string title) =>
                lessons.FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase))?.Id;

            await SeedCatalog.EnsureWordAsync(context, Id("My family"), "family", "/ˈfæməli/", "noun", "A group of parents and children", "gia đình", "This is my family.", "Đây là gia đình tôi.");
            await SeedCatalog.EnsureWordAsync(context, Id("My family"), "mother", "/ˈmʌðə/", "noun", "A female parent", "mẹ", "My mother is a teacher.", "Mẹ tôi là giáo viên.");
            await SeedCatalog.EnsureWordAsync(context, Id("My family"), "brother", "/ˈbrʌðə/", "noun", "A boy who has the same parents", "anh/em trai", "I have one brother.", "Tôi có một em trai.");
            await SeedCatalog.EnsureWordAsync(context, Id("Making plans"), "free", "/friː/", "adjective", "Not busy", "rảnh", "Are you free on Saturday?", "Thứ Bảy bạn rảnh không?");
            await SeedCatalog.EnsureWordAsync(context, Id("Making plans"), "Saturday", "/ˈsætədeɪ/", "noun", "The day after Friday", "thứ Bảy", "Let's meet on Saturday.", "Hãy gặp nhau vào thứ Bảy.");
            await SeedCatalog.EnsureWordAsync(context, Id("Making plans"), "plan", "/plæn/", "noun", "Something you decide to do", "kế hoạch", "I have a plan.", "Tôi có một kế hoạch.");
            await SeedCatalog.EnsureWordAsync(context, Id("At the hotel"), "hotel", "/həʊˈtel/", "noun", "A place you stay when travelling", "khách sạn", "I arrived at the hotel.", "Tôi đến khách sạn.");
            await SeedCatalog.EnsureWordAsync(context, Id("At the hotel"), "reservation", "/ˌrezəˈveɪʃn/", "noun", "A room or table booked in advance", "đặt chỗ", "I have a reservation.", "Tôi đã đặt phòng.");
            await SeedCatalog.EnsureWordAsync(context, Id("At the hotel"), "key", "/kiː/", "noun", "A thing that opens a door", "chìa khóa", "Can I have the key, please?", "Cho tôi xin chìa khóa?");
            await SeedCatalog.EnsureWordAsync(context, Id("At the shop"), "size", "/saɪz/", "noun", "How big something is", "kích cỡ", "Do you have a smaller size?", "Bạn có size nhỏ hơn không?");
            await SeedCatalog.EnsureWordAsync(context, Id("At the shop"), "shirt", "/ʃɜːt/", "noun", "A piece of clothing for the upper body", "áo sơ mi", "I want a blue shirt.", "Tôi muốn một chiếc áo sơ mi xanh.");
            await SeedCatalog.EnsureWordAsync(context, Id("Asking the price"), "price", "/praɪs/", "noun", "How much you pay", "giá", "What is the price?", "Giá bao nhiêu?");
            await SeedCatalog.EnsureWordAsync(context, Id("Asking the price"), "card", "/kɑːd/", "noun", "A bank card used to pay", "thẻ", "Can I pay by card?", "Tôi trả bằng thẻ được không?");
            await SeedCatalog.EnsureWordAsync(context, Id("Asking the price"), "dollar", "/ˈdɒlə/", "noun", "A unit of money", "đô la", "They are two dollars.", "Chúng giá hai đô.");

            await SeedCatalog.EnsureListeningAsync(context, Id("My family"), "/audio/my-family.mp3",
                SeedMedia.ListeningScripts["my-family"],
                ("Who cooks lunch?", "my mother"),
                ("What do they watch?", "a film"));
            await SeedCatalog.EnsureListeningAsync(context, Id("Making plans"), "/audio/making-plans.mp3",
                SeedMedia.ListeningScripts["making-plans"],
                ("Which day?", "Saturday"),
                ("Where do they meet?", "the cafe"));
            await SeedCatalog.EnsureListeningAsync(context, Id("At the hotel"), "/audio/at-the-hotel.mp3",
                SeedMedia.ListeningScripts["at-the-hotel"],
                ("What does he have?", "a reservation"),
                ("Which floor?", "second"));
            await SeedCatalog.EnsureListeningAsync(context, Id("At the shop"), "/audio/at-the-shop.mp3",
                SeedMedia.ListeningScripts["at-the-shop"],
                ("What does she want?", "a shirt"),
                ("How does she pay?", "by card"));
            await SeedCatalog.EnsureListeningAsync(context, Id("Asking the price"), "/audio/asking-the-price.mp3",
                SeedMedia.ListeningScripts["asking-the-price"],
                ("How much are the apples?", "two dollars"),
                ("Can she pay by card?", "yes"));

            await SeedCatalog.EnsureExerciseAsync(context, Id("My family"), "Gia đình",
                new[]
                {
                    ("How do you show a person?", "This is my mother.", "I mother.", "She mother.",
                        "This is + person."),
                    ("I ___ one brother.", "have", "has", "am", "I/you/we/they + have.")
                });
            await SeedCatalog.EnsureExerciseAsync(context, Id("Making plans"), "Lên kế hoạch",
                new[]
                {
                    ("How do you ask if someone is free?", "Are you free on Saturday?", "You free?", "Free you Saturday?",
                        "Are you free + day."),
                    ("Which sentence suggests a plan?", "Let's go to the cafe.", "I am a cafe.", "Cafe please.",
                        "Let's + verb.")
                });
            await SeedCatalog.EnsureExerciseAsync(context, Id("At the hotel"), "Khách sạn",
                new[]
                {
                    ("What do you say at check-in?", "I have a reservation.", "I have a coffee.", "Gate 12 please.",
                        "I have a reservation."),
                    ("How do you ask for the key?", "Can I have the key, please?", "Give key.", "Key now.",
                        "Can I have + noun.")
                });
            await SeedCatalog.EnsureExerciseAsync(context, Id("At the shop"), "Cửa hàng",
                new[]
                {
                    ("How do you ask for another size?", "Do you have this in a smaller size?", "Give small.", "Size you?",
                        "Do you have this in + size."),
                    ("How do you decide to buy it?", "I'll take it.", "I take not.", "It take I.",
                        "I'll take it.")
                });
            await SeedCatalog.EnsureExerciseAsync(context, Id("Asking the price"), "Hỏi giá",
                new[]
                {
                    ("How do you ask the price of apples?", "How much are these apples?", "How many apple?", "Price you?",
                        "How much are + plural."),
                    ("How do you ask to pay by card?", "Can I pay by card?", "I card pay.", "Pay card now you.",
                        "Can I pay by card?")
                });
        }

        private static async Task LinkLessonsAsync(ApplicationDbContext context)
        {
            var lessons = await context.Lessons.ToListAsync();
            var passages = await context.ReadingPassages.ToListAsync();
            var grammar = await context.GrammarTopics.ToListAsync();
            var writing = await context.WritingPrompts.ToListAsync();

            int? P(string title) => passages.FirstOrDefault(x => x.Title == title)?.Id;
            int? G(string title) => grammar.FirstOrDefault(x => x.Title == title)?.Id;
            int? W(string title) => writing.FirstOrDefault(x => x.Title == title)?.Id;

            void Link(string lessonTitle, string? passage, string? topic, string? prompt)
            {
                var lesson = lessons.FirstOrDefault(x => string.Equals(x.Title, lessonTitle, StringComparison.OrdinalIgnoreCase));
                if (lesson == null)
                {
                    return;
                }

                lesson.ReadingPassageId ??= P(passage ?? string.Empty);
                lesson.GrammarTopicId ??= G(topic ?? string.Empty);
                lesson.WritingPromptId ??= W(prompt ?? string.Empty);
            }

            Link("Hello and Introductions", "A morning at the cafe", "Present simple of be", "Introduce yourself");
            Link("Numbers and Time", "A morning at the cafe", "Present simple", "Introduce yourself");
            Link("At the Cafe", "A morning at the cafe", "A / an / the", "A short work email");
            Link("Meeting Friends", "Missed the bus", "Present simple", "A problem on the way");
            Link("At the Airport", "A new airport terminal", "Prepositions of place", "A short work email");
            Link("Asking for Directions", "A new airport terminal", "Prepositions of place", "A problem on the way");
            Link("Writing a short email", "A new airport terminal", "Can for requests", "A short work email");
            Link("In a short meeting", "Why people keep a streak", "First conditional", "Opinion on daily streaks");
            Link("Emphasis and contrast", "Why people keep a streak", "Relative clauses", "Opinion on daily streaks");
            Link("My family", "A day with my family", "Have / has", "My family");
            Link("Making plans", "Saturday plans", "Present continuous", "Weekend plan");
            Link("At the hotel", "Checking in at a hotel", "Can for requests", "Hotel check-in");
            Link("At the shop", "Buying a shirt", "A / an / the", "At the shop");
            Link("Asking the price", "Buying a shirt", "Can for requests", "At the shop");

            await context.SaveChangesAsync();
        }
    }
}
