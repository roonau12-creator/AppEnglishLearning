using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class GrammarProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public int GrammarTopicId { get; set; }

        [Range(0, 100)]
        public int LastPercent { get; set; }

        [Range(0, 100)]
        public int BestPercent { get; set; }

        public int Attempts { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }

        [ForeignKey(nameof(GrammarTopicId))]
        [ValidateNever]
        public GrammarTopic? Topic { get; set; }
    }
}
