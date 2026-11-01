
namespace DesignPatterns.Application.Data.StrategyPattern.Notifications;

public class NotificationService(NotificationSenderFactory senderFactory)
{
    public Task SendNotification(Notification notification, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();
        if (!Enum.IsDefined(notification.Channel))
            throw new ArgumentOutOfRangeException(nameof(notification), notification.Channel, "Unsupported notification channel.");
        return senderFactory(notification.Channel).Send(notification, cancellationToken);
    }
}
