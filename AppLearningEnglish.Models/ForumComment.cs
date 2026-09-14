using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class ForumComment
    {
        [Key]
        public int Id { get; set; }

        public int ForumPostId { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ForumPostId))]
        [ValidateNever]
        public ForumPost? Post { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
