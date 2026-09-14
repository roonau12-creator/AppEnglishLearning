namespace AppLearningEnglish.Business
{
    public static class ExerciseGrading
    {
        /// <summary>
        /// So khớp câu trả lời nhập tay: bỏ qua hoa thường,
        /// dấu câu và khoảng trắng thừa.
        /// </summary>
        public static bool TextAnswerMatches(
            string? expected,
            string? actual)
        {
            var normalizedExpected = NormalizeText(expected);

            return normalizedExpected.Length > 0 &&
                   normalizedExpected == NormalizeText(actual);
        }

        public static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var chars = value
                .ToLowerInvariant()
                .Where(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x));

            return string.Join(
                ' ',
                new string(chars.ToArray())
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
