using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class ListeningQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Listening Lesson")]
        public int ListeningLessonId { get; set; }

        [Required(ErrorMessage = "Question không được để trống")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Answer không được để trống")]
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// FillBlank, MultipleChoice, TranscriptGap, Sentence hoặc Dictation.
        /// </summary>
        [MaxLength(50)]
        public string QuestionType { get; set; } = ListeningQuestionTypes.FillBlank;

        /// <summary>
        /// Các lựa chọn cho dạng MultipleChoice, phân tách bằng dấu |.
        /// </summary>
        public string? Options { get; set; }

        [ForeignKey("ListeningLessonId")]
        [ValidateNever]
        public ListeningLesson? ListeningLesson { get; set; }

        public IReadOnlyList<string> GetOptions()
        {
            if (string.IsNullOrWhiteSpace(Options))
            {
                return Array.Empty<string>();
            }

            return Options
                .Split('|', StringSplitOptions.RemoveEmptyEntries |
                            StringSplitOptions.TrimEntries)
                .ToList();
        }
    }

    public static class ListeningQuestionTypes
    {
        public const string FillBlank = "FillBlank";

        public const string MultipleChoice = "MultipleChoice";

        /// <summary>
        /// Điền từ còn thiếu trong transcript (Question chứa ___ ).
        /// </summary>
        public const string TranscriptGap = "TranscriptGap";

        /// <summary>
        /// Nghe từng câu (Options hoặc Question là câu cần nghe).
        /// </summary>
        public const string Sentence = "Sentence";

        /// <summary>
        /// Nghe rồi gõ lại, chấm chính tả.
        /// </summary>
        public const string Dictation = "Dictation";
    }
}
