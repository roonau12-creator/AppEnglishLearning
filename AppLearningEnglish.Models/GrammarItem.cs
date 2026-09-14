using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class GrammarItem
    {
        [Key]
        public int Id { get; set; }

        public int GrammarTopicId { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        public string? Options { get; set; }

        [Required]
        [MaxLength(255)]
        public string Answer { get; set; } = string.Empty;

        public string? Explanation { get; set; }

        [ForeignKey(nameof(GrammarTopicId))]
        [ValidateNever]
        public GrammarTopic? Topic { get; set; }
    }
}
