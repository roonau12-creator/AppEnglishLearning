using System.ComponentModel.DataAnnotations;

namespace AppLearningEnglish.Models
{
    public class PlacementItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        public string Options { get; set; } = string.Empty;

        [Required]
        [MaxLength(80)]
        public string Answer { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }
}
