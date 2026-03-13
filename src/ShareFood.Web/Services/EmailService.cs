using Microsoft.Extensions.Options;
using ShareFood.Web.Settings;
using System.Net;
using System.Net.Mail;

namespace ShareFood.Web.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            _logger.LogWarning("SMTP chưa được cấu hình. Nội dung thư xác nhận cho {Email}: {Subject}", toEmail, subject);
            _logger.LogInformation("Nội dung thư: {HtmlContent}", htmlContent);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlContent,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(toEmail, toName));

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_settings.UserName)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_settings.UserName, _settings.Password)
        };

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }
}
