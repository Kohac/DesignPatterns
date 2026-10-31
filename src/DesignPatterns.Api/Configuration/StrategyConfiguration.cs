using DesignPatterns.Application.Base.Enums;
using DesignPatterns.Application.Data.StrategyPattern.Notifications;
using DesignPatterns.Application.Data.StrategyPattern.Notifications.Interface;

namespace DesignPatterns.Api.Configuration;

public static class StrategyConfiguration
{
    public static IServiceCollection AddStrategyConfiguration(this IServiceCollection services) => services
        .AddKeyedScoped<INotificationSender, SmsNotificationSender>(NotificationChannel.Sms)
        .AddKeyedScoped<INotificationSender, EmailNotificationSender>(NotificationChannel.Email)
        .AddScoped<NotificationSenderFactory>(provider =>
            channel => provider.GetRequiredKeyedService<INotificationSender>(channel))
        .AddScoped<NotificationService>();
}
