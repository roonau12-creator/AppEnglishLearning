using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppLearningEnglish.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Exercise")]
        public int ExerciseId { get; set; }

        [Required(ErrorMessage = "Question không được để trống")]
        public string QuestionText { get; set; } = string.Empty;

        public string? Explanation { get; set; }

        [ValidateNever]
        [ForeignKey("ExerciseId")]
        public Exercise? Exercise { get; set; }
    }
}