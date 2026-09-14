using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
namespace AppLearningEnglish.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Topic")]
        public int TopicId { get; set; }

        [Required(ErrorMessage = "Tiêu đề Lesson không được để trống")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Content { get; set; }

        public string? VideoUrl { get; set; }

        public string? GrammarNotes { get; set; }

        [Required(ErrorMessage = "Lesson Order không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Lesson Order phải lớn hơn 0")]
        public int LessonOrder { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0")]
        public int DurationMinutes { get; set; }

        public bool IsPublished { get; set; }
        [ForeignKey("CourseId")]
        [ValidateNever]
        public Course? Course { get; set; }
        [ForeignKey("TopicId")]
        [ValidateNever]
        public Topic? Topic { get; set; }

        [ValidateNever]
        public ListeningLesson? ListeningLesson { get; set; }

        [ValidateNever]
        public ICollection<Word> Words { get; set; } = new List<Word>();

        public int? ReadingPassageId { get; set; }

        public int? GrammarTopicId { get; set; }

        public int? WritingPromptId { get; set; }

        [ForeignKey(nameof(ReadingPassageId))]
        [ValidateNever]
        public ReadingPassage? ReadingPassage { get; set; }

        [ForeignKey(nameof(GrammarTopicId))]
        [ValidateNever]
        public GrammarTopic? GrammarTopic { get; set; }

        [ForeignKey(nameof(WritingPromptId))]
        [ValidateNever]
        public WritingPrompt? WritingPrompt { get; set; }
    }
}