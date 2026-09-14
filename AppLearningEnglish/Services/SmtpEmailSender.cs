using System.Net;
using System.Net.Mail;

namespace AppLearningEnglish.Services
{
    public class SmtpEmailSender : IAppEmailSender
    {
        private readonly EmailOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly IWebHostEnvironment _environment;

        public SmtpEmailSender(
            IConfiguration configuration,
            ILogger<SmtpEmailSender> logger,
            IWebHostEnvironment environment)
        {
            _options = configuration.GetSection("Email").Get<EmailOptions>()
                ?? new EmailOptions();
            _logger = logger;
            _environment = environment;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_options.Host) &&
            !string.IsNullOrWhiteSpace(_options.FromAddress);

        public async Task<bool> SendAsync(
            string toEmail,
            string subject,
            string htmlBody)
        {
            if (!IsConfigured)
            {
                await WriteToOutboxAsync(toEmail, subject, htmlBody);
                _logger.LogWarning(
                    "SMTP chưa cấu hình. Email tới {Email} được ghi vào thư mục outbox.",
                    toEmail);
                return false;
            }

            try
            {
                using var client = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = _options.UseSsl,
                    Credentials = string.IsNullOrWhiteSpace(_options.UserName)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(_options.UserName, _options.Password)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_options.FromAddress!, _options.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gửi email tới {Email} thất bại.", toEmail);
                await WriteToOutboxAsync(toEmail, subject, htmlBody);
                return false;
            }
        }

        private async Task WriteToOutboxAsync(
            string toEmail,
            string subject,
            string htmlBody)
        {
            try
            {
                var folder = Path.Combine(
                    _environment.ContentRootPath,
                    "email-outbox");
                Directory.CreateDirectory(folder);

                var fileName =
                    $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}.html";

                await File.WriteAllTextAsync(
                    Path.Combine(folder, fileName),
                    $"<!-- To: {toEmail} | Subject: {subject} -->\n{htmlBody}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không ghi được email vào outbox.");
            }
        }
    }

    public class EmailOptions
    {
        public string? Host { get; set; }

        public int Port { get; set; } = 587;

        public bool UseSsl { get; set; } = true;

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public string? FromAddress { get; set; }

        public string FromName { get; set; } = "AppLearningEnglish";
    }
}
