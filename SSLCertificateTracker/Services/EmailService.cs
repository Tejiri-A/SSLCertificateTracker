using Microsoft.AspNetCore.Identity;

using MailKit.Net.Smtp;
using MimeKit;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string message);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["Email:SenderEmail"]));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };

        using var client = new SmtpClient();
        var portString = _configuration["EmailSettings:SmtpPort"];
        var port = 587;
        if (!string.IsNullOrWhiteSpace(portString) && int.TryParse(portString, out var parsedPort))
        {
            port = parsedPort;
        }

        await client.ConnectAsync(
            _configuration["EmailSettings:SmtpServer"],
            port,
            MailKit.Security.SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            _configuration["EmailSettings:SmtpUsername"],
            _configuration["EmailSettings:SmtpPassword"]);

        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
}