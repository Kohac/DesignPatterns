using System.ComponentModel.DataAnnotations;

namespace DesignPatterns.Application.Data.StrategyPattern.Notifications.Create;

public class NotificationCreateRequest
{
    [Required]
    public required Notification Notification { get; set; }
}
