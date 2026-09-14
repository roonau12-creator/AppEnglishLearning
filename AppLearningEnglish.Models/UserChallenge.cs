using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class UserChallenge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public int ChallengeId { get; set; }

        [Required]
        [MaxLength(20)]
        public string PeriodKey { get; set; } = string.Empty;

        public int Progress { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        [ForeignKey(nameof(ChallengeId))]
        [ValidateNever]
        public Challenge? Challenge { get; set; }
    }
}
