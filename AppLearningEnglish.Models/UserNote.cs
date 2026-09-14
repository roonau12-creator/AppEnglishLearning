using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class UserNote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public int? LessonId { get; set; }

        public int? CourseId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Body { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        [ForeignKey(nameof(LessonId))]
        [ValidateNever]
        public Lesson? Lesson { get; set; }
    }
}
