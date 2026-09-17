namespace EggERP.Application.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}