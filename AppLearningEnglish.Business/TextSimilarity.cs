namespace AppLearningEnglish.Business
{
    public static class TextSimilarity
    {
        public static int Percent(string? expected, string? actual)
        {
            var left = ExerciseGrading.NormalizeText(expected);
            var right = ExerciseGrading.NormalizeText(actual);

            if (left.Length == 0)
            {
                return 0;
            }

            if (right.Length == 0)
            {
                return 0;
            }

            var distance = Distance(left, right);
            var maxLength = Math.Max(left.Length, right.Length);
            return (int)Math.Round((1d - (double)distance / maxLength) * 100);
        }

        public static int Distance(string left, string right)
        {
            var rows = left.Length + 1;
            var cols = right.Length + 1;
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
                    var cost = left[i - 1] == right[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[left.Length, right.Length];
        }
    }
}
