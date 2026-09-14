using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class ReadingPassage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string Level { get; set; } = "Beginner";

        [MaxLength(80)]
        public string? Topic { get; set; }

        [Required]
        public string Body { get; set; } = string.Empty;

        public string? AudioUrl { get; set; }

        public string? TranslationVi { get; set; }

        [ValidateNever]
        public ICollection<ReadingQuestion> Questions { get; set; } = new List<ReadingQuestion>();
    }
}
