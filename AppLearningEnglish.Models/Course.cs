using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AppLearningEnglish.Models
{
      public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên Course không được để trống")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ThumbnailUrl { get; set; }

        [MaxLength(255)]
        public string? Level { get; set; }

        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}