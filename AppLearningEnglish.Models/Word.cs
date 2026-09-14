using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class Word
    {
        [Key]
        public int Id { get; set; }

        public int? LessonId { get; set; }

        [Required(ErrorMessage = "Từ vựng không được để trống")]
        [MaxLength(255)]
        public string WordText { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Pronunciation { get; set; }

        [MaxLength(255)]
        public string? PartOfSpeech { get; set; }

        [MaxLength(255)]
        public string? Definition { get; set; }

        public string? AudioUrl { get; set; }

        public string? ImageUrl { get; set; }

        [MaxLength(10)]
        public string? Level { get; set; }

        [MaxLength(40)]
        public string? TopicSlug { get; set; }

        [MaxLength(255)]
        public string? Synonyms { get; set; }

        [MaxLength(255)]
        public string? Antonyms { get; set; }

        [MaxLength(255)]
        public string? WordFamily { get; set; }

        [ForeignKey(nameof(LessonId))]
        [ValidateNever]
        public Lesson? Lesson { get; set; }

        [ValidateNever]
        public ICollection<WordMeaning> Meanings { get; set; } = new List<WordMeaning>();

        [ValidateNever]
        public ICollection<WordExample> Examples { get; set; } = new List<WordExample>();
    }
}