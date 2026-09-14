using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class Challenge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Period { get; set; } = "daily";

        [MaxLength(200)]
        public string? Description { get; set; }

        public int Target { get; set; } = 1;

        public int RewardCoins { get; set; } = 10;

        public int RewardXp { get; set; } = 20;

        [ValidateNever]
        public ICollection<UserChallenge> UserChallenges { get; set; } = new List<UserChallenge>();
    }
}
