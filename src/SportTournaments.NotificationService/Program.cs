using Microsoft.EntityFrameworkCore;
using SportTournaments.NotificationService.Configuration;
using SportTournaments.NotificationService.Email;
using SportTournaments.NotificationService.Messaging;
using SportTournaments.NotificationService.Persistence;
using SportTournaments.NotificationService.Persistence.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSmtpSettings(builder.Configuration);

builder.Services.AddRabbitMqMessaging(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("NotificationDatabase");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'NotificationDatabase' is not configured.");
}

builder.Services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IEmailDeliveryRepository, EmailDeliveryRepository>();

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

var host = builder.Build();

host.Run();