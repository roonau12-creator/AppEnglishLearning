using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppLearningEnglish.Models
{
    public class WordMeaning
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Word")]
        public int WordId { get; set; }

        [Required(ErrorMessage = "Language không được để trống")]
        [MaxLength(255)]
        public string Language { get; set; } = string.Empty;

        [Required(ErrorMessage = "Meaning không được để trống")]
        public string Meaning { get; set; } = string.Empty;

        [ValidateNever]
        [ForeignKey(nameof(WordId))]
        public Word? Word { get; set; }
    }
}