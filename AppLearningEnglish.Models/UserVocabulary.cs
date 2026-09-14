using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppLearningEnglish.Models
{
    public class UserVocabulary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public int WordId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "New";

        [Range(0, 5)]
        public int Familiarity { get; set; } = 0;

        public int CorrectCount { get; set; } = 0;

        public int WrongCount { get; set; } = 0;

        public DateTime? LastReviewedAt { get; set; }

        public DateTime? NextReviewAt { get; set; }

        public double EaseFactor { get; set; } = 2.5;

        public int IntervalDays { get; set; }

        public int Repetition { get; set; }

        public bool IsFavorite { get; set; }

        public bool IsHard { get; set; }

        [ForeignKey("WordId")]
        [ValidateNever]
        public Word? Word { get; set; }
        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}