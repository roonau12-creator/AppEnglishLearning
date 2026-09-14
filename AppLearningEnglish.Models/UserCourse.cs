using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AppLearningEnglish.Models
{
    public class UserCourse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public int CourseId { get; set; }

        [Range(0, 100)]
        public decimal Progress { get; set; } = 0;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Mã chứng chỉ sinh khi học viên hoàn thành khóa, dùng để tra cứu.
        /// </summary>
        [MaxLength(32)]
        public string? CertificateCode { get; set; }

        [ForeignKey("CourseId")]
        [ValidateNever]
        public Course? Course { get; set; }

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}
