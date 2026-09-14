namespace AppLearningEnglish.Business
{
    public static class EnglishLevelScale
    {
        public const string A1 = "A1";
        public const string A2 = "A2";
        public const string B1 = "B1";
        public const string B2 = "B2";
        public const string C1 = "C1";
        public const string C2 = "C2";

        public const string Beginner = "Beginner";
        public const string Elementary = "Elementary";
        public const string Intermediate = "Intermediate";
        public const string Advanced = "Advanced";

        public static readonly string[] CefrAll = [A1, A2, B1, B2, C1, C2];

        public static readonly string[] All = CefrAll;

        public static int Rank(string? level)
        {
            return Normalize(level) switch
            {
                A1 => 1,
                A2 => 2,
                B1 => 3,
                B2 => 4,
                C1 => 5,
                C2 => 6,
                _ => 0
            };
        }

        public static string Normalize(string? level)
        {
            if (string.IsNullOrWhiteSpace(level))
            {
                return string.Empty;
            }

            var value = level.Trim();

            foreach (var known in CefrAll)
            {
                if (string.Equals(known, value, StringComparison.OrdinalIgnoreCase))
                {
                    return known;
                }
            }

            if (value.Contains("begin", StringComparison.OrdinalIgnoreCase)
                || value.Contains("a1", StringComparison.OrdinalIgnoreCase))
            {
                return A1;
            }

            if (value.Contains("element", StringComparison.OrdinalIgnoreCase)
                || value.Contains("a2", StringComparison.OrdinalIgnoreCase))
            {
                return A2;
            }

            if (value.Contains("b1", StringComparison.OrdinalIgnoreCase)
                || (value.Contains("inter", StringComparison.OrdinalIgnoreCase)
                    && !value.Contains("upper", StringComparison.OrdinalIgnoreCase)))
            {
                return B1;
            }

            if (value.Contains("b2", StringComparison.OrdinalIgnoreCase)
                || value.Contains("upper", StringComparison.OrdinalIgnoreCase))
            {
                return B2;
            }

            if (value.Contains("c1", StringComparison.OrdinalIgnoreCase)
                || value.Contains("adv", StringComparison.OrdinalIgnoreCase))
            {
                return C1;
            }

            if (value.Contains("c2", StringComparison.OrdinalIgnoreCase)
                || value.Contains("profici", StringComparison.OrdinalIgnoreCase))
            {
                return C2;
            }

            return value;
        }

        public static string Display(string? level)
        {
            var normalized = Normalize(level);
            return string.IsNullOrEmpty(normalized) ? "Chưa xếp lớp" : normalized;
        }

        public static bool IsTooHard(string? userLevel, string? contentLevel)
        {
            var userRank = Rank(userLevel);
            var contentRank = Rank(contentLevel);

            if (userRank == 0 || contentRank == 0)
            {
                return false;
            }

            return contentRank > userRank;
        }

        public static string? Next(string? level)
        {
            return Normalize(level) switch
            {
                A1 => A2,
                A2 => B1,
                B1 => B2,
                B2 => C1,
                C1 => C2,
                C2 => C2,
                _ => null
            };
        }

        public static bool IsRecommended(string? userLevel, string? contentLevel)
        {
            var userRank = Rank(userLevel);
            var contentRank = Rank(contentLevel);

            if (userRank == 0 || contentRank == 0)
            {
                return false;
            }

            return contentRank == userRank || contentRank == userRank - 1;
        }
    }
}
