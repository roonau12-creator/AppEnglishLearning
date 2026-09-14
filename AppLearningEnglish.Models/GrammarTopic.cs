using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class GrammarTopic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string Level { get; set; } = "Beginner";

        public int SortOrder { get; set; }

        [Required]
        public string Explanation { get; set; } = string.Empty;

        public string? Examples { get; set; }

        [ValidateNever]
        public ICollection<GrammarItem> Items { get; set; } = new List<GrammarItem>();
    }
}
