using DesignPatterns.Application.Base.Enums;
using DesignPatterns.Application.Data.StrategyPattern.Notifications.Interface;

namespace DesignPatterns.Application.Data.StrategyPattern.Notifications;

public delegate INotificationSender NotificationSenderFactory(NotificationChannel channel);
