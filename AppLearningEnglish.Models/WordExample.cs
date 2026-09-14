using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppLearningEnglish.Models
{
     public class WordExample
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Word")]
        public int WordId { get; set; }

        [Required(ErrorMessage = "Câu tiếng Anh không được để trống")]
        public string EnglishSentence { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nghĩa tiếng Việt không được để trống")]
        public string VietnameseMeaning { get; set; } = string.Empty;

        [ValidateNever]
        [ForeignKey("WordId")]
        public Word? Word { get; set; }
    }
}