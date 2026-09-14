using AppLearningEnglish.Models;

namespace AppLearningEnglish.Business
{
    public static class LessonCompletionHelper
    {
        public static List<string> GetMissingSteps(
            UserLesson? progress,
            bool hasVocab,
            bool hasListening,
            bool hasExercise,
            bool hasReading = false,
            bool hasSpeaking = false,
            bool hasWriting = false,
            bool hasGrammar = false)
        {
            var missing = new List<string>();

            if (hasVocab && progress?.VocabDone != true)
            {
                missing.Add("Vocabulary");
            }

            if (hasListening && progress?.ListeningDone != true)
            {
                missing.Add("Listening");
            }

            if (hasExercise && progress?.ExerciseDone != true)
            {
                missing.Add("Exercise");
            }

            if (hasReading && progress?.ReadingDone != true)
            {
                missing.Add("Reading");
            }

            if (hasSpeaking && progress?.SpeakingDone != true)
            {
                missing.Add("Speaking");
            }

            if (hasWriting && progress?.WritingDone != true)
            {
                missing.Add("Writing");
            }

            if (hasGrammar && progress?.GrammarDone != true)
            {
                missing.Add("Grammar");
            }

            return missing;
        }
    }
}
