namespace DesignPatterns.Application.Data.StrategyPattern.Notifications.Interface;

public interface INotificationSender
{
    Task Send(Notification notification, CancellationToken cancellationToken);
}
