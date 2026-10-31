using DesignPatterns.Application.Data.StrategyPattern.Notifications.Interface;
using Microsoft.Extensions.Logging;

namespace DesignPatterns.Application.Data.StrategyPattern.Notifications;

public class EmailNotificationSender(ILogger<EmailNotificationSender> logger) : INotificationSender
{
    public Task Send(Notification notification, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Simulated Email notification sent.");
        return Task.CompletedTask;
    }
}
