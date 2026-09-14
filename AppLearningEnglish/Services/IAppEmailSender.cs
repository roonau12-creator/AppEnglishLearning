namespace AppLearningEnglish.Services
{
    public interface IAppEmailSender
    {
        bool IsConfigured { get; }

        /// <summary>
        /// Gửi email. Trả về false khi chưa cấu hình SMTP hoặc gửi lỗi,
        /// để màn hình gọi có thể hiện link dự phòng.
        /// </summary>
        Task<bool> SendAsync(
            string toEmail,
            string subject,
            string htmlBody);
    }
}
