using System.ComponentModel.DataAnnotations;

namespace AppLearningEnglish.Models
{
    public class WritingPrompt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(40)]
        public string Level { get; set; } = "Beginner";

        [Required]
        [MaxLength(40)]
        public string Genre { get; set; } = "paragraph";

        [Required]
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Các ý cần viết, cách nhau bằng dấu |.
        /// </summary>
        public string? KeyPoints { get; set; }

        public string? SampleAnswer { get; set; }

        [Range(8, 400)]
        public int MinWords { get; set; } = 40;
    }
}
