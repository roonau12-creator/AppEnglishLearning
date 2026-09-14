using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class PracticeAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Kind { get; set; } = "speaking";

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Prompt { get; set; }

        public string? Expected { get; set; }

        public string? UserAnswer { get; set; }

        [Range(0, 100)]
        public int Score { get; set; }

        public string? Feedback { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
