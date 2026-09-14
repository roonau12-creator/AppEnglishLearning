using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AppLearningEnglish.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        [MaxLength(255)]
        public string? EnglishLevel { get; set; }

        public string? AvatarUrl { get; set; }

        public IFormFile? AvatarFile { get; set; }

        [Range(5, 480, ErrorMessage = "Mục tiêu mỗi ngày từ 5 đến 480 phút")]
        public int DailyGoalMinutes { get; set; } = 30;

        [Range(1, 200, ErrorMessage = "Số từ ôn mỗi ngày từ 1 đến 200")]
        public int VocabDailyTarget { get; set; } = 10;

        [Range(25, 400, ErrorMessage = "Khoảng ôn từ 25% đến 400%")]
        public int ReviewIntervalPercent { get; set; } = 100;

        public bool ReminderEnabled { get; set; } = true;

        [Range(0, 23)]
        public int ReminderHour { get; set; } = 19;

        [MaxLength(80)]
        public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";

        [MaxLength(20)]
        public string PreferredTheme { get; set; } = "light";

        [MaxLength(20)]
        public string LearningGoalKind { get; set; } = "vocab";

        [Range(50, 20000)]
        public int VocabGoalTotal { get; set; } = 2000;

        [Range(0, 9)]
        public decimal? IeltsBandTarget { get; set; }

        [MaxLength(8)]
        public string UiLanguage { get; set; } = "vi";
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class SearchResultViewModel
    {
        public string? Query { get; set; }

        public List<Course> Courses { get; set; } = new();

        public List<Lesson> Lessons { get; set; } = new();

        public List<Word> Words { get; set; } = new();

        public List<ReadingPassage> Passages { get; set; } = new();

        public List<GrammarTopic> GrammarTopics { get; set; } = new();

        public List<WritingPrompt> WritingPrompts { get; set; } = new();
    }
}
