using AppLearningEnglish.Business;
using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Identity
{
    internal static class SeedProductExperience
    {
        public static async Task EnsureAsync(ApplicationDbContext context)
        {
            await EnsureTopicsAsync(context);
            await EnsureChallengesAsync(context);
            await EnsureGrammarTreeAsync(context);
            await EnsureWritingGenresAsync(context);
            await EnrichWordsAsync(context);
            await EnrichListeningAsync(context);
            await EnrichReadingAsync(context);
        }

        private static async Task EnsureTopicsAsync(ApplicationDbContext context)
        {
            var existing = await context.topics.Select(x => x.Name).ToListAsync();
            var missing = VocabTopics.All
                .Where(x => !existing.Any(name =>
                    string.Equals(name, x.Name, StringComparison.OrdinalIgnoreCase)))
                .Select(x => new Topic { Name = x.Name, Description = $"Chủ đề {x.Name}" })
                .ToList();

            if (missing.Count > 0)
            {
                context.topics.AddRange(missing);
                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureChallengesAsync(ApplicationDbContext context)
        {
            if (await context.Challenges.AnyAsync())
            {
                return;
            }

            context.Challenges.AddRange(
                new Challenge
                {
                    Code = "daily-minutes",
                    Title = "Học 15 phút hôm nay",
                    Period = "daily",
                    Description = "Daily challenge",
                    Target = 15,
                    RewardCoins = 10,
                    RewardXp = 20
                },
                new Challenge
                {
                    Code = "daily-quiz",
                    Title = "Làm 1 quiz",
                    Period = "daily",
                    Description = "Daily quiz",
                    Target = 1,
                    RewardCoins = 8,
                    RewardXp = 15
                },
                new Challenge
                {
                    Code = "daily-xp",
                    Title = "Kiếm 20 XP",
                    Period = "daily",
                    Target = 20,
                    RewardCoins = 6,
                    RewardXp = 10
                },
                new Challenge
                {
                    Code = "weekly-xp",
                    Title = "100 XP trong tuần",
                    Period = "weekly",
                    Target = 100,
                    RewardCoins = 40,
                    RewardXp = 50
                },
                new Challenge
                {
                    Code = "monthly-minutes",
                    Title = "300 phút trong tháng",
                    Period = "monthly",
                    Target = 300,
                    RewardCoins = 80,
                    RewardXp = 100
                });

            await context.SaveChangesAsync();
        }

        private static async Task EnsureGrammarTreeAsync(ApplicationDbContext context)
        {
            var titles = await context.GrammarTopics.Select(x => x.Title).ToListAsync();

            void AddIfMissing(string title, string level, int order, string explanation, string examples, params (string Q, string Opt, string A, string Why)[] items)
            {
                if (titles.Any(x => string.Equals(x, title, StringComparison.OrdinalIgnoreCase)))
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
                context.SaveChanges();
                foreach (var item in items)
                {
                    context.GrammarItems.Add(new GrammarItem
                    {
                        GrammarTopicId = topic.Id,
                        QuestionText = item.Q,
                        Options = item.Opt,
                        Answer = item.A,
                        Explanation = item.Why
                    });
                }
            }

            AddIfMissing("There is / There are", "A1", 10,
                "There is + singular. There are + plural. Câu hỏi: Is there / Are there?",
                "There is a book on the table.\nThere are two cups.",
                ("There ___ a cafe near here.", "is|are|am|be", "is", "Cafe số ít."),
                ("There ___ three tickets.", "is|are|am|be", "are", "Tickets số nhiều."));

            AddIfMissing("Can / Can't", "A1", 11,
                "Can + V để nói khả năng hoặc xin phép. Phủ định: cannot / can't.",
                "I can swim.\nCan I sit here?",
                ("She ___ speak English.", "can|cans|is can|can to", "can", "can + V nguyên mẫu."),
                ("___ I open the window?", "Do|Can|Am|Have", "Can", "Xin phép dùng Can I."));

            AddIfMissing("Present Continuous", "A1", 12,
                "am/is/are + V-ing cho hành động đang xảy ra.",
                "I am studying now.\nThey are waiting.",
                ("He ___ working now.", "is|are|am|be", "is", "He + is."),
                ("We ___ having lunch.", "is|are|am|be", "are", "We + are."));

            AddIfMissing("Future Simple", "A2", 13,
                "will + V cho quyết định lúc nói hoặc dự đoán.",
                "I will call you.\nIt will rain.",
                ("I ___ help you.", "will|am|do|can will", "will", "will + V."),
                ("___ she come tomorrow?", "Does|Will|Is|Has", "Will", "Câu hỏi tương lai: Will + S."));

            AddIfMissing("Comparatives", "A2", 14,
                "Tính từ ngắn + er. Tính từ dài: more + adj. So sánh bằng: as ... as.",
                "This bag is cheaper.\nThis hotel is more expensive.",
                ("This room is ___ than that one.", "big|bigger|biggest|more big", "bigger", "short adj + er."),
                ("This test is ___ difficult.", "more|most|er|very", "more", "more + long adj."));

            AddIfMissing("Present Perfect", "A2", 15,
                "have/has + V3. Dùng cho trải nghiệm và việc còn liên quan hiện tại.",
                "I have visited Hue.\nShe has just arrived.",
                ("I ___ seen that film.", "have|has|had|am", "have", "I + have."),
                ("She has ___ the email.", "send|sent|sending|sends", "sent", "V3 của send là sent."));

            AddIfMissing("Modals of advice", "B1", 16,
                "should / shouldn't + V để khuyên. ought to ít thông dụng hơn.",
                "You should rest.\nYou shouldn't drink coffee at night.",
                ("You ___ see a doctor.", "should|should to|must to|are", "should", "should + V."),
                ("He shouldn't ___ so late.", "works|work|working|worked", "work", "shouldn't + V."));

            AddIfMissing("Second conditional", "B2", 17,
                "If + past simple, would + V cho giả định hiện tại.",
                "If I had time, I would travel.",
                ("If I ___ rich, I would buy a house.", "am|was|were|be", "were", "If I were..."),
                ("She would call if she ___ your number.", "knows|knew|known|know", "knew", "If + past."));

            AddIfMissing("Passive voice", "B2", 18,
                "be + V3. The report was sent yesterday.",
                "English is spoken here.\nThe email was written by Minh.",
                ("The room ___ cleaned every day.", "is|are|was being|be", "is", "Hiện tại bị động: is + V3."),
                ("The tickets were ___ online.", "buy|bought|buying|buys", "bought", "were + V3."));

            AddIfMissing("Cleft sentences", "C1", 19,
                "It is/was ... that/who để nhấn mạnh.",
                "It was Lan who called.\nIt is the budget that we need.",
                ("It was the manager ___ signed the paper.", "which|who|when|where", "who", "who cho người."),
                ("It is practice ___ helps most.", "who|that|whose|whom", "that", "that cho vật/ý."));

            AddIfMissing("Inversion", "C2", 20,
                "Đảo ngữ sau phủ định: Hardly had I arrived when...",
                "Never have I seen such rain.\nOnly then did we understand.",
                ("Never ___ I heard that story.", "have|did|had|do", "have", "Never have + S + V3."),
                ("Only then ___ we see the problem.", "do|did|have|are", "did", "Only then + did + S."));

            await context.SaveChangesAsync();
        }

        private static async Task EnsureWritingGenresAsync(ApplicationDbContext context)
        {
            var prompts = await context.WritingPrompts.ToListAsync();
            foreach (var prompt in prompts)
            {
                if (string.IsNullOrWhiteSpace(prompt.Genre) || prompt.Genre == "paragraph")
                {
                    prompt.Genre = prompt.Title.Contains("email", StringComparison.OrdinalIgnoreCase)
                        ? "email"
                        : prompt.Title.Contains("Opinion", StringComparison.OrdinalIgnoreCase)
                            ? "essay"
                            : "paragraph";
                }
            }

            if (!prompts.Any(x => x.Genre == "essay" && x.Title.Contains("City", StringComparison.OrdinalIgnoreCase)))
            {
                context.WritingPrompts.AddRange(
                    new WritingPrompt
                    {
                        Title = "Essay: City or countryside",
                        Level = "B2",
                        Genre = "essay",
                        Prompt = "Viết essay ngắn: sống ở thành phố hay nông thôn tốt hơn? Nêu 2 lý do và kết luận.",
                        KeyPoints = "city|countryside|however|because",
                        SampleAnswer = "Living in a city is convenient because there are jobs and schools. However, the countryside is quieter. I prefer the city because I can study and work.",
                        MinWords = 80
                    },
                    new WritingPrompt
                    {
                        Title = "Write a sentence: daily routine",
                        Level = "A1",
                        Genre = "sentence",
                        Prompt = "Viết 2–3 câu về buổi sáng của bạn.",
                        KeyPoints = "morning|breakfast|I",
                        SampleAnswer = "I get up at 6. I have breakfast and go to school.",
                        MinWords = 12
                    });
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnrichWordsAsync(ApplicationDbContext context)
        {
            var words = await context.words.Include(x => x.Lesson).ThenInclude(x => x!.Course).ToListAsync();
            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word.Level))
                {
                    word.Level = EnglishLevelScale.Normalize(word.Lesson?.Course?.Level);
                    if (string.IsNullOrWhiteSpace(word.Level))
                    {
                        word.Level = "A1";
                    }
                }

                if (string.IsNullOrWhiteSpace(word.TopicSlug))
                {
                    var course = word.Lesson?.Course?.Name ?? "";
                    word.TopicSlug = course.Contains("Shop", StringComparison.OrdinalIgnoreCase) ? "shopping"
                        : course.Contains("Work", StringComparison.OrdinalIgnoreCase) ? "work"
                        : course.Contains("Travel", StringComparison.OrdinalIgnoreCase) ? "travel"
                        : course.Contains("Family", StringComparison.OrdinalIgnoreCase) ? "family"
                        : "daily-routines";
                }

                if (string.IsNullOrWhiteSpace(word.Synonyms))
                {
                    word.Synonyms = GuessRelated(word.WordText, "syn");
                }

                if (string.IsNullOrWhiteSpace(word.Antonyms))
                {
                    word.Antonyms = GuessRelated(word.WordText, "ant");
                }

                if (string.IsNullOrWhiteSpace(word.WordFamily))
                {
                    word.WordFamily = word.WordText;
                }
            }

            await context.SaveChangesAsync();
        }

        private static string GuessRelated(string word, string kind)
        {
            return word.ToLowerInvariant() switch
            {
                "good" => kind == "ant" ? "bad" : "great, fine",
                "big" => kind == "ant" ? "small" : "large",
                "happy" => kind == "ant" ? "sad" : "glad",
                "buy" => kind == "ant" ? "sell" : "purchase",
                "arrive" => kind == "ant" ? "leave" : "reach",
                "early" => kind == "ant" ? "late" : "soon",
                _ => kind == "ant" ? "" : word
            };
        }

        private static async Task EnrichListeningAsync(ApplicationDbContext context)
        {
            var lessons = await context.ListeningLessons.ToListAsync();
            foreach (var lesson in lessons)
            {
                if (string.IsNullOrWhiteSpace(lesson.TranslationVi) && !string.IsNullOrWhiteSpace(lesson.Transcript))
                {
                    lesson.TranslationVi = "Bản dịch gợi ý: đọc transcript rồi đối chiếu nghĩa từng câu với bài học.";
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnrichReadingAsync(ApplicationDbContext context)
        {
            var passages = await context.ReadingPassages.ToListAsync();
            foreach (var passage in passages)
            {
                passage.Level = string.IsNullOrWhiteSpace(EnglishLevelScale.Normalize(passage.Level))
                    ? passage.Level
                    : EnglishLevelScale.Normalize(passage.Level);

                if (string.IsNullOrWhiteSpace(passage.TranslationVi))
                {
                    passage.TranslationVi = "Bản dịch đoạn: đây là bản gợi ý. Hãy highlight từ mới và lưu vào sổ từ.";
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
