using AppLearningEnglish.Business.Services;
using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business
{
    public static class SpeechFeedback
    {
        public static SpeechAssessment Assess(
            string? expected,
            string? heard,
            string? pronunciation = null,
            int? durationMs = null)
        {
            return new PronunciationScoringService().Score(expected, heard, pronunciation, durationMs);
        }
    }
}
