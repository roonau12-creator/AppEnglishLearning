using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using AppLearningEnglish.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppLearningEnglish.Models
{
    public class UserLesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public int LessonId { get; set; }

        public bool IsCompleted { get; set; } = false;

        [Range(0, 100)]
        public decimal Progress { get; set; } = 0;

        public DateTime? CompletedAt { get; set; }

        public bool VocabDone { get; set; }

        public bool ListeningDone { get; set; }

        public bool ExerciseDone { get; set; }

        public bool ReadingDone { get; set; }

        public bool SpeakingDone { get; set; }

        public bool WritingDone { get; set; }

        public bool GrammarDone { get; set; }

        [ForeignKey("LessonId")]
        // Navigation
        [ValidateNever]
        public Lesson? Lesson { get; set; }
        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}