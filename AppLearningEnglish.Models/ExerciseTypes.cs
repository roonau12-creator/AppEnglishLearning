namespace AppLearningEnglish.Models
{
    public static class ExerciseTypes
    {
        public const string MultipleChoice = "MultipleChoice";

        public const string FillBlank = "FillBlank";

        public const string TrueFalse = "TrueFalse";

        public const string Matching = "Matching";

        public const string Listening = "Listening";

        public const string Reading = "Reading";

        /// <summary>
        /// Đưa giá trị Type về dạng chuẩn không dấu cách.
        /// Dữ liệu cũ từng lưu "Multiple Choice", "Fill in the Blank"...
        /// </summary>
        public static string Normalize(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return MultipleChoice;
            }

            var compact = new string(
                type.Where(x => !char.IsWhiteSpace(x)).ToArray());

            foreach (var known in All)
            {
                if (string.Equals(compact, known, StringComparison.OrdinalIgnoreCase))
                {
                    return known;
                }
            }

            if (compact.Contains("blank", StringComparison.OrdinalIgnoreCase))
            {
                return FillBlank;
            }

            if (compact.Contains("truefalse", StringComparison.OrdinalIgnoreCase))
            {
                return TrueFalse;
            }

            return MultipleChoice;
        }

        public static readonly string[] All =
        {
            MultipleChoice,
            FillBlank,
            TrueFalse,
            Matching,
            Listening,
            Reading
        };

        /// <summary>
        /// Dạng hiển thị cho người học, quyết định cách render đề bài.
        /// </summary>
        public static ExerciseRenderMode GetRenderMode(string? type) =>
            Normalize(type) switch
            {
                FillBlank => ExerciseRenderMode.Text,
                TrueFalse => ExerciseRenderMode.TrueFalse,
                Matching => ExerciseRenderMode.Matching,
                Listening or Reading => ExerciseRenderMode.PassageChoice,
                _ => ExerciseRenderMode.Choice
            };

        public static string GetDisplayName(string? type) =>
            Normalize(type) switch
            {
                FillBlank => "Điền từ",
                TrueFalse => "Đúng / Sai",
                Matching => "Nối cặp",
                Listening => "Nghe hiểu",
                Reading => "Đọc hiểu",
                _ => "Trắc nghiệm"
            };
    }

    public enum ExerciseRenderMode
    {
        Choice,

        Text,

        TrueFalse,

        Matching,

        PassageChoice
    }
}
