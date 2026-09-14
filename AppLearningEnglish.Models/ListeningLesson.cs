using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class ListeningLesson
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Lesson")]
        public int LessonId { get; set; }

        public string? AudioUrl { get; set; }

        public string? Transcript { get; set; }

        public string? TranslationVi { get; set; }

        public string? VideoUrl { get; set; }

        [ForeignKey("LessonId")]
        [ValidateNever]
        public Lesson? Lesson { get; set; }

        // Không tạo cột database
        [NotMapped]
        public IFormFile? AudioFile { get; set; }
    }
}