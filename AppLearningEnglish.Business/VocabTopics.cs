namespace AppLearningEnglish.Business
{
    public static class VocabTopics
    {
        public static readonly (string Slug, string Name)[] All =
        [
            ("greetings", "Greetings"),
            ("family", "Family"),
            ("friends", "Friends"),
            ("daily-routines", "Daily Routines"),
            ("hobbies", "Hobbies"),
            ("food", "Food"),
            ("travel", "Travel"),
            ("school", "School"),
            ("work", "Work"),
            ("technology", "Technology"),
            ("health", "Health"),
            ("shopping", "Shopping"),
            ("sports", "Sports"),
            ("environment", "Environment"),
            ("business", "Business")
        ];

        public static string Name(string? slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return "Khác";
            }

            var match = All.FirstOrDefault(x =>
                string.Equals(x.Slug, slug, StringComparison.OrdinalIgnoreCase));

            return string.IsNullOrEmpty(match.Name) ? slug : match.Name;
        }
    }
}
