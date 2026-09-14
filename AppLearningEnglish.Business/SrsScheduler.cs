using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business
{
    /// <summary>
    /// SuperMemo-2 spaced repetition, scaled by the learner's interval percent.
    /// </summary>
    public static class SrsScheduler
    {
        public const int MinIntervalPercent = 25;

        public const int MaxIntervalPercent = 400;

        public const double DefaultEase = 2.5;

        public static void ApplyReview(UserVocabulary item, int quality, int intervalPercent = 100)
        {
            quality = Math.Clamp(quality, 0, 5);
            if (item.EaseFactor < 1.3)
            {
                item.EaseFactor = DefaultEase;
            }

            if (quality < 3)
            {
                item.Repetition = 0;
                item.IntervalDays = 0;
                item.Familiarity = Math.Max(0, item.Familiarity - 1);
                item.NextReviewAt = DateTime.UtcNow.AddMinutes(10);
            }
            else
            {
                if (item.Repetition == 0)
                {
                    item.IntervalDays = 1;
                }
                else if (item.Repetition == 1)
                {
                    item.IntervalDays = 6;
                }
                else
                {
                    item.IntervalDays = Math.Max(1, (int)Math.Round(item.IntervalDays * item.EaseFactor));
                }

                item.Repetition++;
                item.Familiarity = Math.Min(5, Math.Max(item.Familiarity + 1, item.Repetition));
                var days = item.IntervalDays * (ClampPercent(intervalPercent) / 100d);
                item.NextReviewAt = DateTime.UtcNow.AddDays(Math.Max(0.04, days));
            }

            var easeDelta = 0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02);
            item.EaseFactor = Math.Clamp(item.EaseFactor + easeDelta, 1.3, 3.0);
            item.LastReviewedAt = DateTime.UtcNow;
            item.Status = GetStatus(item.Familiarity, item.IntervalDays, item.Repetition);
        }

        public static DateTime CalculateNextReview(
            DateTime now,
            int familiarity,
            int intervalPercent = 100)
        {
            double hours = familiarity switch
            {
                <= 0 => 6,
                1 => 24,
                2 => 48,
                3 => 96,
                4 => 168,
                _ => 336
            };

            return now.AddHours(hours * ClampPercent(intervalPercent) / 100d);
        }

        public static int ClampPercent(int intervalPercent)
        {
            if (intervalPercent < MinIntervalPercent)
            {
                return MinIntervalPercent;
            }

            return intervalPercent > MaxIntervalPercent
                ? MaxIntervalPercent
                : intervalPercent;
        }

        public static string GetStatus(int familiarity) =>
            GetStatus(familiarity, 0, familiarity);

        public static string GetStatus(int familiarity, int intervalDays, int repetition)
        {
            if (familiarity <= 0 && repetition == 0)
            {
                return "New";
            }

            if (intervalDays >= 21 && familiarity >= 4)
            {
                return "Mastered";
            }

            return familiarity switch
            {
                <= 2 => "Learning",
                <= 4 => "Reviewing",
                _ => "Mastered"
            };
        }
    }
}
