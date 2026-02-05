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
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["EmailSettings:SenderEmail"]));
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

        var options = port == 465 
            ? MailKit.Security.SecureSocketOptions.SslOnConnect 
            : MailKit.Security.SecureSocketOptions.StartTls;

        var smtpServer = _configuration["EmailSettings:SmtpServer"]?.Trim();
        if (string.IsNullOrEmpty(smtpServer))
        {
            throw new InvalidOperationException("SMTP Server is not configured in EmailSettings:SmtpServer");
        }

        _logger.LogInformation("Attempting to connect to SMTP server: {SmtpServer} on port {Port} with options {Options}", smtpServer, port, options);

        try
        {
            await client.ConnectAsync(
                smtpServer,
                port,
                options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to SMTP server {SmtpServer}", smtpServer);
            throw;
        }

        await client.AuthenticateAsync(
            _configuration["EmailSettings:SmtpUsername"],
            _configuration["EmailSettings:SmtpPassword"]);

        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
}