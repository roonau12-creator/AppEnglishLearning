using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppLearningEnglish.Models.ViewModels
{
     public class ExerciseQuestionViewModel
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; }
            = string.Empty;

        public List<ExerciseAnswerViewModel> Answers { get; set; }
            = new List<ExerciseAnswerViewModel>();
    }

    public class ExerciseAnswerViewModel
    {
        public int AnswerId { get; set; }

        public string AnswerText { get; set; }
            = string.Empty;
    }

    public class DoExerciseViewModel
    {
        public int AttemptId { get; set; }

        public int ExerciseId { get; set; }

        public string ExerciseName { get; set; }
            = string.Empty;

        public string ExerciseType { get; set; }
            = ExerciseTypes.MultipleChoice;

        public ExerciseRenderMode RenderMode =>
            ExerciseTypes.GetRenderMode(ExerciseType);

        public bool IsFillBlank =>
            RenderMode == ExerciseRenderMode.Text;

        /// <summary>
        /// Đoạn văn / lời dẫn cho dạng Reading và Listening.
        /// </summary>
        public string? Passage { get; set; }

        /// <summary>
        /// Kho đáp án dùng chung cho dạng Matching.
        /// </summary>
        public List<ExerciseAnswerViewModel> MatchingPool { get; set; }
            = new List<ExerciseAnswerViewModel>();

        public List<ExerciseQuestionViewModel> Questions { get; set; }
            = new List<ExerciseQuestionViewModel>();
    }

    public class SubmitExerciseViewModel
    {
        public int AttemptId { get; set; }

        public int ExerciseId { get; set; }

        public Dictionary<int, int> Answers { get; set; }
            = new Dictionary<int, int>();

        public Dictionary<int, string> TextAnswers { get; set; }
            = new Dictionary<int, string>();
    }


    public class ExerciseResultReviewViewModel
    {
        public ExerciseAttempt Attempt { get; set; } = null!;

        public int? LessonId { get; set; }

        public List<ExerciseResultQuestionViewModel> Questions { get; set; } = new();
    }

    public class ExerciseResultQuestionViewModel
    {
        public string QuestionText { get; set; } = string.Empty;

        public string? Explanation { get; set; }

        public bool IsFillBlank { get; set; }

        public string? UserText { get; set; }

        public string? CorrectText { get; set; }

        public bool IsCorrect { get; set; }

        public List<ExerciseResultAnswerViewModel> Answers { get; set; } = new();
    }

    public class ExerciseResultAnswerViewModel
    {
        public string AnswerText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public bool IsSelected { get; set; }
    }
}