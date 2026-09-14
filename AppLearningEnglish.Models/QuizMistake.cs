using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class QuizMistake
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Kind { get; set; } = "vocab";

        [Required]
        public string Prompt { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Expected { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? UserAnswer { get; set; }

        public bool IsReviewed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
