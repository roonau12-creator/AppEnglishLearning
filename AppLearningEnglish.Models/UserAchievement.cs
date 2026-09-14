using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class UserAchievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int AchievementId { get; set; }

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AchievementId")]
        [ValidateNever]
        public Achievement? Achievement { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
