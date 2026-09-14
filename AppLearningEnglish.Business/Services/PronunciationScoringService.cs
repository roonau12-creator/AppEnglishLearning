using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business.Services
{
    /// <summary>
    /// Chấm phát âm theo từng từ bằng Levenshtein: khớp, gần đúng, sai.
    /// Dùng transcript từ Web Speech API (trình duyệt), không gọi API trả phí.
    /// </summary>
    public class PronunciationScoringService : IPronunciationScoringService
    {
        public SpeechAssessment Score(
            string? expected,
            string? heard,
            string? pronunciation = null,
            int? durationMs = null)
        {
            var expectedWords = Tokenize(expected);
            var heardWords = Tokenize(heard);

            if (expectedWords.Count == 0)
            {
                return new SpeechAssessment();
            }

            var marks = Align(expectedWords, heardWords);
            var scored = marks.Where(x => x.Status != "extra").ToList();
            var accuracy = scored.Count == 0
                ? 0
                : (int)Math.Round(scored.Average(x => x.Similarity));

            var completeness = scored.Count == 0
                ? 0
                : (int)Math.Round(100d * scored.Count(x => x.Status is "match" or "near") / scored.Count);

            if (heardWords.Count == 0)
            {
                completeness = 0;
            }

            ApplyNearMissHints(marks);

            var stress = ScoreStress(pronunciation, marks);
            var intonation = ScoreIntonation(expected, heard, marks);
            var fluency = ScoreFluency(expectedWords.Count, durationMs);
            var extraPenalty = Math.Min(15, marks.Count(x => x.Status == "extra") * 4);

            var overall = (int)Math.Round(
                accuracy * 0.55
                + completeness * 0.2
                + fluency * 0.12
                + stress * 0.08
                + intonation * 0.05);
            overall = Math.Clamp(overall - extraPenalty, 0, 100);

            var tips = BuildTips(marks, expected, pronunciation, durationMs, expectedWords.Count);

            return new SpeechAssessment
            {
                Overall = overall,
                Accuracy = accuracy,
                Completeness = completeness,
                Fluency = fluency,
                Stress = stress,
                Intonation = intonation,
                Words = marks,
                Tips = tips
            };
        }

        internal static List<string> Tokenize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return [];
            }

            return text
                .ToLowerInvariant()
                .Split([' ', '\n', '\r', '\t', ',', '.', '!', '?', ';', ':', '"', '\'', '“', '”', '-', '—'],
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToList();
        }

        /// <summary>
        /// Căn từ bằng Levenshtein trên chuỗi từ; chi phí thay thế = khoảng cách ký tự (0..1)
        /// để ưu tiên ghép lỗi gần đúng (ship/sheep) thay vì coi là thiếu/thừa.
        /// </summary>
        internal static List<SpeechWordMark> Align(IReadOnlyList<string> expected, IReadOnlyList<string> heard)
        {
            var rows = expected.Count + 1;
            var cols = heard.Count + 1;
            var matrix = new double[rows, cols];

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
                    var subCost = SubstituteCost(expected[i - 1], heard[j - 1]);
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + subCost);
                }
            }

            var marks = new List<SpeechWordMark>();
            var x = expected.Count;
            var y = heard.Count;

            while (x > 0 || y > 0)
            {
                if (x > 0 && y > 0)
                {
                    var subCost = SubstituteCost(expected[x - 1], heard[y - 1]);
                    if (AlmostEqual(matrix[x, y], matrix[x - 1, y - 1] + subCost))
                    {
                        marks.Add(MarkPair(expected[x - 1], heard[y - 1]));
                        x--;
                        y--;
                        continue;
                    }
                }

                if (y > 0 && (x == 0 || AlmostEqual(matrix[x, y], matrix[x, y - 1] + 1)))
                {
                    marks.Add(new SpeechWordMark
                    {
                        Expected = string.Empty,
                        Heard = heard[y - 1],
                        Status = "extra",
                        Distance = heard[y - 1].Length,
                        Similarity = 0
                    });
                    y--;
                    continue;
                }

                marks.Add(new SpeechWordMark
                {
                    Expected = expected[x - 1],
                    Heard = null,
                    Status = "missing",
                    Distance = expected[x - 1].Length,
                    Similarity = 0
                });
                x--;
            }

            marks.Reverse();
            return marks;
        }

        internal static SpeechWordMark MarkPair(string expected, string heard)
        {
            var left = ExerciseGrading.NormalizeText(expected);
            var right = ExerciseGrading.NormalizeText(heard);
            var distance = TextSimilarity.Distance(left, right);
            var maxLen = Math.Max(left.Length, right.Length);
            var similarity = maxLen == 0
                ? 100
                : (int)Math.Round((1d - (double)distance / maxLen) * 100);

            string status;
            if (distance == 0)
            {
                status = "match";
                similarity = 100;
            }
            else if (IsNearMiss(distance, maxLen, similarity))
            {
                status = "near";
                similarity = Math.Max(similarity, 55);
            }
            else
            {
                status = "wrong";
            }

            return new SpeechWordMark
            {
                Expected = expected,
                Heard = heard,
                Status = status,
                Distance = distance,
                Similarity = similarity
            };
        }

        private static bool IsNearMiss(int distance, int maxLen, int similarity)
        {
            if (similarity >= 60)
            {
                return true;
            }

            if (maxLen <= 4)
            {
                return distance == 1;
            }

            return distance <= 2;
        }

        private static double SubstituteCost(string expected, string heard)
        {
            var left = ExerciseGrading.NormalizeText(expected);
            var right = ExerciseGrading.NormalizeText(heard);
            if (left == right)
            {
                return 0;
            }

            var maxLen = Math.Max(Math.Max(left.Length, right.Length), 1);
            return Math.Clamp((double)TextSimilarity.Distance(left, right) / maxLen, 0.15, 1);
        }

        private static bool AlmostEqual(double left, double right) =>
            Math.Abs(left - right) < 0.0001;

        private static void ApplyNearMissHints(List<SpeechWordMark> marks)
        {
            foreach (var mark in marks.Where(x => x.Status is "near" or "wrong"))
            {
                var pairHint = ConfusionHint(mark.Expected, mark.Heard);
                if (!string.IsNullOrWhiteSpace(pairHint))
                {
                    mark.PhoneticHint = pairHint;
                    continue;
                }

                if (mark.Status == "near")
                {
                    mark.PhoneticHint =
                        $"Gần đúng (Levenshtein {mark.Distance}): cần “{mark.Expected}”, hệ thống nghe “{mark.Heard}”.";
                }
                else
                {
                    mark.PhoneticHint =
                        $"Sai từ: cần “{mark.Expected}”, nghe được “{mark.Heard}”.";
                }
            }
        }

        private static string? ConfusionHint(string? expected, string? heard)
        {
            var left = (expected ?? string.Empty).ToLowerInvariant();
            var right = (heard ?? string.Empty).ToLowerInvariant();
            if (left.Length == 0 || right.Length == 0)
            {
                return null;
            }

            if (HasSwap(left, right, 'l', 'r'))
            {
                return $"L / R: lưỡi chạm nướu cho “{expected}”.";
            }

            if (left.Contains("th") || right.Contains("th"))
            {
                return $"TH: lưỡi chạm răng cửa khi nói “{expected}”.";
            }

            if (HasSwap(left, right, 'v', 'w') || HasSwap(left, right, 'b', 'v'))
            {
                return "V / W / B: cắn nhẹ môi dưới cho /v/, tròn môi cho /w/.";
            }

            return null;
        }

        private static bool HasSwap(string left, string right, char a, char b) =>
            (left.Contains(a) && right.Contains(b)) || (left.Contains(b) && right.Contains(a));

        private static int ScoreStress(string? pronunciation, List<SpeechWordMark> marks)
        {
            var expectedMarks = marks.Where(x => x.Status != "extra").ToList();
            if (expectedMarks.Count == 0)
            {
                return 0;
            }

            var fromWords = (int)Math.Round(expectedMarks.Average(x => x.Similarity));
            if (!string.IsNullOrWhiteSpace(pronunciation) && pronunciation.Contains('ˈ'))
            {
                return Math.Min(fromWords, expectedMarks.Any(x => x.Status is "missing" or "wrong") ? 72 : 94);
            }

            return fromWords;
        }

        private static int ScoreIntonation(string? expected, string? heard, List<SpeechWordMark> marks)
        {
            var expectedQuestion = LooksLikeQuestion(expected);
            var heardQuestion = LooksLikeQuestion(heard);
            var orderScore = marks.Count(x => x.Status != "extra") == 0
                ? 0
                : (int)Math.Round(marks.Where(x => x.Status != "extra").Average(x => x.Similarity));

            return expectedQuestion == heardQuestion
                ? Math.Max(orderScore, 78)
                : Math.Min(orderScore, 58);
        }

        private static bool LooksLikeQuestion(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (text.Contains('?'))
            {
                return true;
            }

            var first = Tokenize(text).FirstOrDefault();
            return first is "what" or "where" or "when" or "why" or "who" or "how"
                or "is" or "are" or "do" or "does" or "did" or "can" or "could"
                or "would" or "will";
        }

        private static int ScoreFluency(int wordCount, int? durationMs)
        {
            if (wordCount <= 0)
            {
                return 0;
            }

            if (durationMs is null or <= 0)
            {
                return 76;
            }

            var ideal = Math.Max(700, wordCount * 420);
            var ratio = (double)durationMs.Value / ideal;

            if (ratio is >= 0.7 and <= 1.4)
            {
                return 94;
            }

            if (ratio is >= 0.5 and <= 1.8)
            {
                return 78;
            }

            return 58;
        }

        private static List<string> BuildTips(
            List<SpeechWordMark> marks,
            string? expected,
            string? pronunciation,
            int? durationMs,
            int expectedCount)
        {
            var tips = new List<string>();

            foreach (var mark in marks.Where(x => x.Status != "match"))
            {
                if (mark.Status == "missing")
                {
                    tips.Add($"Thiếu từ “{mark.Expected}”.");
                }
                else if (mark.Status == "extra")
                {
                    tips.Add($"Thừa từ “{mark.Heard}”.");
                }
                else if (!string.IsNullOrWhiteSpace(mark.PhoneticHint))
                {
                    tips.Add(mark.PhoneticHint);
                }
            }

            if (!string.IsNullOrWhiteSpace(pronunciation) && pronunciation.Contains('ˈ'))
            {
                var index = pronunciation.IndexOf('ˈ');
                var after = pronunciation[(index + 1)..];
                var syllable = new string(after.TakeWhile(ch => ch is not '.' and not ' ' and not '/').ToArray());
                tips.Add(string.IsNullOrWhiteSpace(syllable)
                    ? $"Trọng âm chính nằm sau dấu ˈ trong {pronunciation.Trim()}."
                    : $"Trọng âm chính rơi vào “{syllable}”. Nhấn mạnh âm tiết này.");
            }

            if (LooksLikeQuestion(expected))
            {
                tips.Add("Đây là câu hỏi — nhấn giọng lên ở cuối câu.");
            }

            if (durationMs is > 0 && expectedCount > 0)
            {
                var fast = durationMs < expectedCount * 280;
                var slow = durationMs > expectedCount * 700;
                if (fast)
                {
                    tips.Add("Bạn nói hơi nhanh. Ngắt nhẹ giữa các từ.");
                }
                else if (slow)
                {
                    tips.Add("Bạn nói hơi chậm. Giữ nhịp tự nhiên hơn.");
                }
            }

            if (tips.Count == 0)
            {
                tips.Add("Khớp tốt. Giữ nhịp này.");
            }

            return tips.Distinct().Take(6).ToList();
        }
    }
}
