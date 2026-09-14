using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class ForumPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public int LikeCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        [ValidateNever]
        public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
    }
}
