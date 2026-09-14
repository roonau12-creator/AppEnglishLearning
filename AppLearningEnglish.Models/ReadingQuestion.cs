using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class ReadingQuestion
    {
        [Key]
        public int Id { get; set; }

        public int ReadingPassageId { get; set; }

        [Required]
        [MaxLength(40)]
        public string Type { get; set; } = "Detail";

        [Required]
        public string Prompt { get; set; } = string.Empty;

        public string? Options { get; set; }

        [Required]
        [MaxLength(255)]
        public string Answer { get; set; } = string.Empty;

        public string? Explanation { get; set; }

        [ForeignKey(nameof(ReadingPassageId))]
        [ValidateNever]
        public ReadingPassage? Passage { get; set; }
    }
}
