
namespace DesignPatterns.Application.Data.StrategyPattern.Notifications;

public class NotificationService(NotificationSenderFactory senderFactory)
{
    public Task SendNotification(Notification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();
        return senderFactory(notification.Channel).Send(notification, cancellationToken);
    }
}
