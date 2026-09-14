using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Question")]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Answer không được để trống")]
        public string AnswerText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
        [ForeignKey("QuestionId")]
        [ValidateNever]
        public Question? Question { get; set; }
    }
}