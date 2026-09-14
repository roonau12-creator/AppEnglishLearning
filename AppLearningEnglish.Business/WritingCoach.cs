using AppLearningEnglish.Models.ViewModels;
using System.Text.RegularExpressions;

namespace AppLearningEnglish.Business
{
    public static class WritingCoach
    {
        private static readonly (string Wrong, string Right, string Note)[] CommonFixes =
        [
            ("i ", "I ", "Viết hoa đại từ I."),
            (" im ", " I'm ", "Dùng I'm thay vì im."),
            (" dont ", " don't ", "Thiếu dấu nháy: don't."),
            (" cant ", " can't ", "Thiếu dấu nháy: can't."),
            (" wont ", " won't ", "Thiếu dấu nháy: won't."),
            (" thats ", " that's ", "Thiếu dấu nháy: that's."),
            (" its a ", " it's a ", "it's = it is; its là sở hữu."),
            (" your welcome ", " you're welcome ", "you're = you are."),
            (" there house ", " their house ", "their là sở hữu."),
            (" they are is ", " they are ", "Không cần is sau they are.")
        ];

        public static WritingReview ReviewRewrite(string? expected, string? actual)
        {
            var answer = (actual ?? string.Empty).Trim();
            var model = (expected ?? string.Empty).Trim();
            var meaning = TextSimilarity.Percent(model, answer);
            var grammarNotes = CollectGrammarNotes(answer, model);
            var grammar = Math.Max(0, 100 - grammarNotes.Count * 12);
            var diff = BuildDiff(model, answer);

            var comments = new List<string>();
            if (meaning >= 85)
            {
                comments.Add("Nghĩa đã gần câu mẫu.");
            }
            else if (meaning >= 60)
            {
                comments.Add("Ý đúng hướng nhưng còn lệch từ hoặc ngữ pháp.");
            }
            else
            {
                comments.Add("Câu chưa đủ ý so với đề. Đọc lại gợi ý tiếng Việt rồi viết đủ chủ ngữ + động từ.");
            }

            comments.AddRange(grammarNotes);

            var overall = (int)Math.Round(meaning * 0.55 + grammar * 0.45);

            return new WritingReview
            {
                Overall = Math.Clamp(overall, 0, 100),
                Grammar = grammar,
                Meaning = meaning,
                Coverage = meaning,
                Corrections = grammarNotes,
                Comments = comments.Distinct().Take(8).ToList(),
                Diff = diff,
                SuggestedRewrite = string.IsNullOrWhiteSpace(model) ? null : model
            };
        }

        public static WritingReview ReviewFree(
            string? answer,
            IEnumerable<string>? keyPoints,
            string? sample,
            int minWords)
        {
            var text = (answer ?? string.Empty).Trim();
            var words = Tokenize(text);
            var points = (keyPoints ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            var hit = points.Count == 0
                ? 1
                : points.Count(point => ContainsPoint(text, point));
            var coverage = points.Count == 0
                ? (words.Count >= minWords ? 80 : 50)
                : (int)Math.Round(100d * hit / points.Count);

            var grammarNotes = CollectGrammarNotes(text, sample);
            grammarNotes.AddRange(SpellChecker.FindIssues(text));
            if (words.Count < Math.Max(8, minWords))
            {
                grammarNotes.Insert(0, $"Bài hơi ngắn ({words.Count} từ). Hãy viết tối thiểu {Math.Max(8, minWords)} từ.");
            }

            var grammar = Math.Max(0, 100 - grammarNotes.Count * 10);
            var meaning = string.IsNullOrWhiteSpace(sample)
                ? coverage
                : Math.Max(coverage, TextSimilarity.Percent(sample, text) / 2 + coverage / 2);

            var comments = new List<string>
            {
                points.Count == 0
                    ? $"Độ dài: {words.Count} từ."
                    : $"Đã chạm {hit}/{points.Count} ý cần có."
            };
            comments.AddRange(grammarNotes);

            if (hit < points.Count)
            {
                var missing = points.Where(point => !ContainsPoint(text, point)).Take(3);
                comments.Add("Còn thiếu ý: " + string.Join("; ", missing) + ".");
            }

            var overall = (int)Math.Round(coverage * 0.4 + grammar * 0.35 + meaning * 0.25);

            return new WritingReview
            {
                Overall = Math.Clamp(overall, 0, 100),
                Grammar = grammar,
                Meaning = meaning,
                Coverage = coverage,
                Corrections = grammarNotes,
                Comments = comments.Distinct().Take(8).ToList(),
                Diff = string.IsNullOrWhiteSpace(sample) ? [] : BuildDiff(sample, text),
                SuggestedRewrite = sample
            };
        }

        private static bool ContainsPoint(string text, string point)
        {
            var haystack = " " + ExerciseGrading.NormalizeText(text) + " ";
            var needles = point.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return needles.Any(needle => haystack.Contains(" " + ExerciseGrading.NormalizeText(needle) + " "));
        }

        private static List<string> CollectGrammarNotes(string answer, string? expected)
        {
            var notes = new List<string>();
            var trimmed = answer.Trim();

            if (trimmed.Length == 0)
            {
                notes.Add("Chưa có câu trả lời.");
                return notes;
            }

            if (!char.IsUpper(trimmed[0]) && !trimmed.StartsWith("I "))
            {
                notes.Add("Viết hoa chữ cái đầu câu.");
            }

            if (trimmed[^1] is not ('.' or '?' or '!'))
            {
                notes.Add("Thêm dấu chấm hoặc dấu hỏi ở cuối câu.");
            }

            if (Regex.IsMatch(trimmed, @"\bi\b"))
            {
                notes.Add("Đại từ I luôn viết hoa.");
            }

            if (Regex.IsMatch(" " + trimmed.ToLowerInvariant() + " ", @"\b(he|she|it)\s+(go|do|have|want|like|need|make|take|come|say|know|think|live|work)\b"))
            {
                notes.Add("Ngôi he/she/it cần động từ thêm -s ở hiện tại đơn (goes, likes, lives…).");
            }

            if (Regex.IsMatch(trimmed, @"\ba\s+[aeiouAEIOU]"))
            {
                notes.Add("Trước nguyên âm dùng an, không dùng a (an apple).");
            }

            if (Regex.IsMatch(trimmed, @"\ban\s+[bcdfghjklmnpqrstvwxyzBCDFGHJKLMNPQRSTVWXYZ]"))
            {
                notes.Add("Trước phụ âm dùng a, không dùng an (a book).");
            }

            if (Regex.IsMatch(trimmed, @"\b(I|we|they|you)\s+(is|was)\b", RegexOptions.IgnoreCase))
            {
                notes.Add("I/we/they/you không đi với is/was. Dùng am/are/were.");
            }

            if (Regex.IsMatch(trimmed, @"\b(he|she|it)\s+(are|were)\b", RegexOptions.IgnoreCase))
            {
                notes.Add("He/she/it đi với is/was, không dùng are/were.");
            }

            if (Regex.IsMatch(trimmed, @"\b(yesterday|last night|last week)\b.+\b(go|see|have|meet|come|buy)\b", RegexOptions.IgnoreCase)
                && !Regex.IsMatch(trimmed, @"\b(went|saw|had|met|came|bought|did)\b", RegexOptions.IgnoreCase))
            {
                notes.Add("Có mốc quá khứ (yesterday/last…) — động từ chính nên ở quá khứ.");
            }

            var sentences = Regex.Split(trimmed, @"(?<=[\.!\?])\s+").Where(x => x.Length > 0).ToList();
            if (sentences.Count >= 1 && sentences.Any(x => Tokenize(x).Count > 28))
            {
                notes.Add("Có câu quá dài. Tách thành 2 câu ngắn, mỗi câu một ý.");
            }

            var lower = trimmed.ToLowerInvariant();
            var cohesion = new[] { "because", "so", "but", "however", "although", "then", "also" };
            if (Tokenize(trimmed).Count >= 40 && cohesion.Count(w => lower.Contains(w)) == 0)
            {
                notes.Add("Bài hơi dài mà ít từ nối. Thêm because / but / however / then để mạch lạc.");
            }

            if (Regex.IsMatch(trimmed, @"\b(very very|and and|I I)\b", RegexOptions.IgnoreCase))
            {
                notes.Add("Có từ lặp. Bỏ bớt từ trùng.");
            }

            var padded = " " + trimmed.ToLowerInvariant() + " ";
            foreach (var (wrong, right, note) in CommonFixes)
            {
                if (padded.Contains(wrong))
                {
                    notes.Add($"{note} Gợi ý: {right.Trim()}.");
                }
            }

            if (!string.IsNullOrWhiteSpace(expected))
            {
                var expectedTokens = Tokenize(expected);
                var actualTokens = Tokenize(trimmed);
                var missing = expectedTokens
                    .Except(actualTokens, StringComparer.OrdinalIgnoreCase)
                    .Where(x => x.Length > 2)
                    .Take(4)
                    .ToList();

                if (missing.Count > 0 && actualTokens.Count > 0)
                {
                    notes.Add("Có thể thiếu từ: " + string.Join(", ", missing) + ".");
                }
            }

            if (notes.Count == 0 && trimmed.Length > 20)
            {
                notes.Add("Ngữ pháp bề mặt ổn. Đọc lại câu mẫu và thêm 1 chi tiết cụ thể.");
            }

            return notes.Distinct().ToList();
        }

        private static List<WritingDiffWord> BuildDiff(string expected, string actual)
        {
            var left = Tokenize(expected);
            var right = Tokenize(actual);
            var marks = new List<WritingDiffWord>();

            var rows = left.Count + 1;
            var cols = right.Count + 1;
            var matrix = new int[rows, cols];

            for (var i = 0; i < rows; i++)
            {
                matrix[i, 0] = i;
            }

            for (var j = 0; j < cols; j++)
            {
                matrix[0, j] = j;
            }

            for (var i = 1; i < rows; i++)
            {
                for (var j = 1; j < cols; j++)
                {
                    var cost = string.Equals(left[i - 1], right[j - 1], StringComparison.OrdinalIgnoreCase) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            var x = left.Count;
            var y = right.Count;

            while (x > 0 || y > 0)
            {
                if (x > 0 && y > 0 && string.Equals(left[x - 1], right[y - 1], StringComparison.OrdinalIgnoreCase))
                {
                    marks.Add(new WritingDiffWord { Text = right[y - 1], Status = "match" });
                    x--;
                    y--;
                    continue;
                }

                if (x > 0 && y > 0 && matrix[x, y] == matrix[x - 1, y - 1] + 1)
                {
                    marks.Add(new WritingDiffWord { Text = right[y - 1], Status = "wrong", Expected = left[x - 1] });
                    x--;
                    y--;
                    continue;
                }

                if (y > 0 && (x == 0 || matrix[x, y] == matrix[x, y - 1] + 1))
                {
                    marks.Add(new WritingDiffWord { Text = right[y - 1], Status = "extra" });
                    y--;
                    continue;
                }

                marks.Add(new WritingDiffWord { Text = left[x - 1], Status = "missing" });
                x--;
            }

            marks.Reverse();
            return marks;
        }

        private static List<string> Tokenize(string text)
        {
            return Regex.Matches(text.ToLowerInvariant(), "[a-z0-9']+")
                .Select(x => x.Value)
                .ToList();
        }
    }
}
