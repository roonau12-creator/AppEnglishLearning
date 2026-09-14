using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppLearningEnglish.Models
{
    public class Exercise
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Lesson")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại Exercise")]
        [MaxLength(255)]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Question không được để trống")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Exercise Order không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Exercise Order phải lớn hơn 0")]
        public int ExerciseOrder { get; set; }

        [ValidateNever]
        [ForeignKey("LessonId")]
        public Lesson? Lesson { get; set; }
    }
}