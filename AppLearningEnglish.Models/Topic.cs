using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AppLearningEnglish.Models
{
    public class Topic
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên Topic không được để trống")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

    }
}