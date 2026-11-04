using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SportTournaments.NotificationService.Configuration;

namespace SportTournaments.NotificationService.Email;

public sealed class SmtpEmailSender(IOptions<SmtpSettings> smtpOptions, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpSettings _settings = smtpOptions.Value;

    public async Task SendAsync(string recipientEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        message.From.Add(MailboxAddress.Parse(_settings.FromAddress));

        message.To.Add(MailboxAddress.Parse(recipientEmail));

        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to connect to SMTP server {Host}:{Port}.",
                _settings.Host,
                _settings.Port);

            throw;
        }

        logger.LogInformation("SMTP connection established. IsConnected: {IsConnected}.", client.IsConnected);

        await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

        await client.SendAsync(message, cancellationToken);

        await client.DisconnectAsync(true, cancellationToken);
    }
}