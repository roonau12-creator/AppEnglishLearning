using AppLearningEnglish.Models.ViewModels;

namespace AppLearningEnglish.Business.Services.IServices
{
    public interface IPronunciationScoringService
    {
        SpeechAssessment Score(
            string? expected,
            string? heard,
            string? pronunciation = null,
            int? durationMs = null);
    }
}
