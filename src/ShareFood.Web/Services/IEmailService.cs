namespace ShareFood.Web.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, CancellationToken cancellationToken = default);
}
