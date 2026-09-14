using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class DailyStreak
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public DateOnly StudyDate { get; set; }

        [Required]
        public int MinutesStudied { get; set; }

        [Required]
        public int PointsEarned { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
