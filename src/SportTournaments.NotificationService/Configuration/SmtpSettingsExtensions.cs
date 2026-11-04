using System.Net.Mail;

namespace SportTournaments.NotificationService.Configuration;

public static class SmtpSettingsExtensions
{
    public static IServiceCollection AddSmtpSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SmtpSettings>()
            .Bind(configuration.GetSection(SmtpSettings.SectionName))
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Host), "SMTP host must be configured.")
            .Validate(settings => settings.Port is > 0 and <= 65535, "SMTP port must be between 1 and 65535.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Username), "SMTP username must be configured.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.Password), "SMTP password must be configured.")
            .Validate(settings => MailAddress.TryCreate(settings.FromAddress, out _), "SMTP sender address must be a valid email address.")
            .ValidateOnStart();

        return services;
    }
}