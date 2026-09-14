using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AppLearningEnglish.Models
{
    public class Achievement
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên Achievement không được để trống.")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả không được để trống.")]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        public string? IconUrl { get; set; }

        [Range(0, 1000000)]
        public int RequiredPoints { get; set; }

    }
}