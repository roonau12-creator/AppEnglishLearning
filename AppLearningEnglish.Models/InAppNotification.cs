using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class InAppNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string Kind { get; set; } = "reminder";

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(400)]
        public string? Body { get; set; }

        [MaxLength(200)]
        public string? Link { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
