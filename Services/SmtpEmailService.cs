using System.Net;
using System.Net.Mail;
using PromptMarketPlace.Services.Interfaces;

namespace PromptMarketPlace.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var section = _config.GetSection("Email");
        var host = section["SmtpHost"] ?? "localhost";
        var port = section.GetValue("SmtpPort", 587);
        var useSsl = section.GetValue("UseSsl", true);
        var username = section["Username"] ?? "";
        var password = section["Password"] ?? "";
        var senderEmail = section["SenderEmail"] ?? "noreply@promptmarket.ir";
        var senderName = section["SenderName"] ?? "پرامپت مارکت";

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = useSsl,
                Credentials = string.IsNullOrEmpty(username)
                    ? null
                    : new NetworkCredential(username, password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
            // اجازه می‌دهیم اجرا ادامه یابد — ارسال ایمیل critical نیست
        }
    }
}
