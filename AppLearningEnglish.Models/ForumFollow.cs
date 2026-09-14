using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class ForumFollow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FollowerId { get; set; } = string.Empty;

        [Required]
        public string FollowedUserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(FollowerId))]
        [ValidateNever]
        public ApplicationUser? Follower { get; set; }

        [ForeignKey(nameof(FollowedUserId))]
        [ValidateNever]
        public ApplicationUser? FollowedUser { get; set; }
    }
}
