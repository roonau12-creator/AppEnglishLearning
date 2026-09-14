using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business
{
    public static class FreeTalkEngine
    {
        public static ConversationTurn Next(string? lastUserReply, int turnIndex)
        {
            var text = (lastUserReply ?? string.Empty).ToLowerInvariant();

            if (turnIndex == 0)
            {
                return new ConversationTurn
                {
                    Tutor = "Hello! How are you today? What would you like to talk about?",
                    Hint = "Chào và nêu chủ đề: food, travel, work hoặc study.",
                    Keywords = ["fine", "good", "food", "travel", "work", "study", "hello"],
                    Sample = "I'm good, thank you. I want to talk about travel."
                };
            }

            if (ContainsAny(text, "food", "eat", "coffee", "lunch", "dinner"))
            {
                return new ConversationTurn
                {
                    Tutor = "Nice! What is your favorite food, and how often do you cook?",
                    Hint = "Nói món thích và tần suất.",
                    Keywords = ["like", "favorite", "cook", "often", "usually"],
                    Sample = "I like pho. I usually cook at the weekend."
                };
            }

            if (ContainsAny(text, "travel", "trip", "airport", "hotel", "city"))
            {
                return new ConversationTurn
                {
                    Tutor = "Where did you last travel, and what did you enjoy there?",
                    Hint = "Kể chuyến đi gần nhất.",
                    Keywords = ["went", "last", "enjoy", "city", "visited"],
                    Sample = "I went to Da Nang last year. I enjoyed the beach."
                };
            }

            if (ContainsAny(text, "work", "job", "office", "meeting", "boss"))
            {
                return new ConversationTurn
                {
                    Tutor = "What do you do at work, and what is a typical day like?",
                    Hint = "Mô tả công việc một ngày.",
                    Keywords = ["work", "office", "meeting", "email", "day"],
                    Sample = "I work in an office. I write emails and join short meetings."
                };
            }

            return new ConversationTurn
            {
                Tutor = "That is interesting. Can you tell me one more sentence with a reason? Use because.",
                Hint = "Thêm because + lý do.",
                Keywords = ["because", "so", "think"],
                Sample = "I study English because I want a better job."
            };
        }

        private static bool ContainsAny(string text, params string[] keys) =>
            keys.Any(key => text.Contains(key, StringComparison.OrdinalIgnoreCase));
    }
}
