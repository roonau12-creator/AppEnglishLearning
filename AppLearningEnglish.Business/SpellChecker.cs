using System.Text.RegularExpressions;

namespace AppLearningEnglish.Business
{
    public static class SpellChecker
    {
        private static readonly Dictionary<string, string> Common =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["recieve"] = "receive",
                ["occured"] = "occurred",
                ["seperate"] = "separate",
                ["definately"] = "definitely",
                ["tommorow"] = "tomorrow",
                ["tommorrow"] = "tomorrow",
                ["becuase"] = "because",
                ["untill"] = "until",
                ["wich"] = "which",
                ["teh"] = "the",
                ["thier"] = "their",
                ["freind"] = "friend",
                ["enviroment"] = "environment",
                ["goverment"] = "government",
                ["buisness"] = "business",
                ["accomodation"] = "accommodation",
                ["adress"] = "address",
                ["begining"] = "beginning",
                ["beleive"] = "believe",
                ["calender"] = "calendar",
                ["completly"] = "completely",
                ["exmaple"] = "example",
                ["grammer"] = "grammar",
                ["intersting"] = "interesting",
                ["langauge"] = "language",
                ["neccessary"] = "necessary",
                ["practise"] = "practice",
                ["recomend"] = "recommend",
                ["sucess"] = "success",
                ["writting"] = "writing"
            };

        public static IReadOnlyList<string> FindIssues(string? text)
        {
            var notes = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return notes;
            }

            foreach (Match match in Regex.Matches(text, @"[A-Za-z']+"))
            {
                var word = match.Value;
                if (Common.TryGetValue(word, out var correct)
                    && !string.Equals(word, correct, StringComparison.OrdinalIgnoreCase))
                {
                    notes.Add($"Chính tả: “{word}” → “{correct}”.");
                }
            }

            return notes.Distinct().Take(8).ToList();
        }
    }
}
