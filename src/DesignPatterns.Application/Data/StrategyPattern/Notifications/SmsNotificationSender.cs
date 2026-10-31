using DesignPatterns.Application.Data.StrategyPattern.Notifications.Interface;
using Microsoft.Extensions.Logging;

namespace DesignPatterns.Application.Data.StrategyPattern.Notifications;

public class SmsNotificationSender(ILogger<SmsNotificationSender> logger) : INotificationSender
{
    public Task Send(Notification notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Simulated Sms notification sent.");
        return Task.CompletedTask;
    }
}
