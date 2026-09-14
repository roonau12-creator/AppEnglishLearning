using AppLearningEnglish.Models;
using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business
{
    public static class CoachEngine
    {
        public static string Example(Word word, string? level)
        {
            var text = word.WordText;
            var cefr = EnglishLevelScale.Normalize(level);
            return cefr switch
            {
                EnglishLevelScale.A1 or EnglishLevelScale.A2 =>
                    $"I use the word \"{text}\" every day.",
                EnglishLevelScale.B1 or EnglishLevelScale.B2 =>
                    $"People often use \"{text}\" when they talk about {word.TopicSlug ?? "daily life"}.",
                _ => $"A precise way to use \"{text}\" is in a formal sentence about {word.TopicSlug ?? "your goal"}."
            };
        }

        public static string Translate(string? text)
        {
            var value = (text ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                return "Nhập câu cần dịch.";
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["hello"] = "xin chào",
                ["thank you"] = "cảm ơn",
                ["how are you"] = "bạn khỏe không",
                ["i am a student"] = "tôi là học sinh"
            };

            return map.TryGetValue(value, out var vi)
                ? $"{value} → {vi}"
                : $"Gợi ý dịch (từ điển ngắn): “{value}”. Với câu dài hãy dùng từ điển chuyên, đây không phải mô hình dịch.";
        }

        public static string SynonymDiff(string left, string right)
        {
            return $"“{left}” và “{right}” có thể gần nghĩa nhưng khác ngữ cảnh. {left} thường trung tính hơn; {right} có thể mạnh hoặc trang trọng hơn. Hãy xem collocation trong câu mẫu.";
        }

        public static string Story(IEnumerable<string> words)
        {
            var list = words.Where(x => !string.IsNullOrWhiteSpace(x)).Take(8).ToList();
            if (list.Count == 0)
            {
                return "Hãy thêm từ vào sổ rồi tạo truyện.";
            }

            return $"Lan had a small {list[0]} in the morning. She wanted to { (list.Count > 1 ? list[1] : "study") } before work. " +
                   $"At the cafe she said hello and used these words: {string.Join(", ", list)}. " +
                   "Then she went home and reviewed the same words again.";
        }

        public static string Answer(string? question, IEnumerable<GrammarTopic> topics)
        {
            var q = (question ?? string.Empty).Trim();
            if (q.Length == 0)
            {
                return "Hãy hỏi một cấu trúc (ví dụ: present perfect, can, if).";
            }

            var hit = topics.FirstOrDefault(x =>
                q.Contains(x.Title, StringComparison.OrdinalIgnoreCase)
                || (x.Explanation?.Contains(q, StringComparison.OrdinalIgnoreCase) == true));

            if (hit != null)
            {
                return $"{hit.Title} ({hit.Level})\n{hit.Explanation}\nVí dụ:\n{hit.Examples}";
            }

            return "Chưa khớp chủ điểm trong ngân hàng ngữ pháp. Thử từ khóa: present simple, past, can, if, relative.";
        }
    }
}
