using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppLearningEnglish.Models
{
     public class ExerciseAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public int ExerciseId { get; set; }

        [Range(0, 100)]
        public decimal Score { get; set; }

        public int CorrectAnswers { get; set; }

        public int TotalQuestions { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string? SelectedAnswersJson { get; set; }

        /// <summary>
        /// Câu trả lời dạng nhập chữ (FillBlank), JSON theo QuestionId.
        /// </summary>
        public string? TextAnswersJson { get; set; }

        [ForeignKey("ExerciseId")]
        [ValidateNever]
        public Exercise? Exercise { get; set; }
        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

    }
}