using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business
{
    public static class PlacementBank
    {
        public static IReadOnlyList<PlacementQuestion> Questions { get; } =
        [
            new()
            {
                Id = 1,
                Text = "I ___ a student.",
                Options = ["am", "is", "are", "be"],
                Answer = "am"
            },
            new()
            {
                Id = 2,
                Text = "She ___ to school every day.",
                Options = ["go", "goes", "going", "gone"],
                Answer = "goes"
            },
            new()
            {
                Id = 3,
                Text = "There are ___ apples on the table.",
                Options = ["a", "an", "some", "any"],
                Answer = "some"
            },
            new()
            {
                Id = 4,
                Text = "I have lived here ___ 2020.",
                Options = ["for", "since", "in", "at"],
                Answer = "since"
            },
            new()
            {
                Id = 5,
                Text = "If I ___ rich, I would travel more.",
                Options = ["am", "were", "was", "be"],
                Answer = "were"
            },
            new()
            {
                Id = 6,
                Text = "He suggested ___ early.",
                Options = ["to leave", "leaving", "leave", "left"],
                Answer = "leaving"
            },
            new()
            {
                Id = 7,
                Text = "This book ___ by millions of people.",
                Options = ["reads", "is read", "was reading", "has read"],
                Answer = "is read"
            },
            new()
            {
                Id = 8,
                Text = "Neither of them ___ ready yet.",
                Options = ["are", "is", "be", "were"],
                Answer = "is"
            },
            new()
            {
                Id = 9,
                Text = "I'd rather you ___ now.",
                Options = ["leave", "left", "leaving", "to leave"],
                Answer = "left"
            },
            new()
            {
                Id = 10,
                Text = "Hardly ___ the train left when it started raining.",
                Options = ["had", "has", "did", "was"],
                Answer = "had"
            }
        ];

        public static string LevelForScore(int score, int total = 10)
        {
            if (total <= 0)
            {
                return "Beginner";
            }

            var percent = 100d * score / total;

            if (percent < 20)
            {
                return EnglishLevelScale.A1;
            }

            if (percent < 35)
            {
                return EnglishLevelScale.A2;
            }

            if (percent < 50)
            {
                return EnglishLevelScale.B1;
            }

            if (percent < 70)
            {
                return EnglishLevelScale.B2;
            }

            if (percent < 85)
            {
                return EnglishLevelScale.C1;
            }

            return EnglishLevelScale.C2;
        }
    }
}
