namespace DesignPatterns.Tests;

using DesignPatterns.Api.Configuration;
using DesignPatterns.Application.Base.Enums;
using DesignPatterns.Application.Data.StrategyPattern.Notifications;
using DesignPatterns.Application.Data.StrategyPattern.Notifications.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class NotificationTests
{
    [Theory]
    [InlineData(NotificationChannel.Email)]
    [InlineData(NotificationChannel.Sms)]
    public async Task Service_selects_sender_and_forwards_notification_and_token(NotificationChannel channel)
    {
        var sender = new RecordingSender();
        NotificationChannel? selectedChannel = null;
        var service = new NotificationService(selected =>
        {
            selectedChannel = selected;
            return sender;
        });
        var notification = new Notification("recipient", "subject", "message", channel);
        using var cancellation = new CancellationTokenSource();

        await service.SendNotification(notification, cancellation.Token);

        Assert.Equal(channel, selectedChannel);
        Assert.Same(notification, sender.Notification);
        Assert.Equal(cancellation.Token, sender.Token);
    }

    [Fact]
    public async Task Unknown_channel_is_rejected_before_factory_is_called()
    {
        var service = new NotificationService(_ => throw new InvalidOperationException("Factory must not run."));
        var notification = new Notification("recipient", "subject", "message", (NotificationChannel)999);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SendNotification(notification, default));
    }

    [Fact]
    public async Task Cancelled_request_is_rejected_before_factory_is_called()
    {
        var service = new NotificationService(_ => throw new InvalidOperationException("Factory must not run."));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SendNotification(
            new Notification("recipient", "subject", "message", NotificationChannel.Email), cancellation.Token));
    }

    [Theory]
    [InlineData(NotificationChannel.Email, typeof(EmailNotificationSender))]
    [InlineData(NotificationChannel.Sms, typeof(SmsNotificationSender))]
    public async Task Production_registration_resolves_and_runs_correct_scoped_sender(NotificationChannel channel, Type senderType)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStrategyConfiguration();
        await using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        using IServiceScope scope = provider.CreateScope();
        NotificationSenderFactory factory = scope.ServiceProvider.GetRequiredService<NotificationSenderFactory>();
        INotificationSender sender = factory(channel);
        Assert.IsType(senderType, sender);
        Assert.Same(sender, factory(channel));
        using IServiceScope otherScope = provider.CreateScope();
        Assert.NotSame(sender, otherScope.ServiceProvider.GetRequiredService<NotificationSenderFactory>()(channel));

        await scope.ServiceProvider.GetRequiredService<NotificationService>().SendNotification(
            new Notification("recipient", "subject", "message", channel), default);
    }

    [Theory]
    [InlineData(NotificationChannel.Email)]
    [InlineData(NotificationChannel.Sms)]
    public async Task Senders_log_simulation_and_honor_cancellation(NotificationChannel channel)
    {
        var logger = new RecordingLogger();
        INotificationSender sender = channel == NotificationChannel.Email
            ? new EmailNotificationSender(logger)
            : new SmsNotificationSender(logger);
        var notification = new Notification("recipient", "subject", "message", channel);
        await sender.Send(notification, default);
        Assert.Contains(channel.ToString(), Assert.Single(logger.Messages));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sender.Send(notification, cancellation.Token));
        Assert.Single(logger.Messages);
    }

    private sealed class RecordingSender : INotificationSender
    {
        public Notification? Notification { get; private set; }
        public CancellationToken Token { get; private set; }
        public Task Send(Notification notification, CancellationToken cancellationToken)
        {
            Notification = notification;
            Token = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingLogger : ILogger<EmailNotificationSender>, ILogger<SmsNotificationSender>
    {
        public List<string> Messages { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add(formatter(state, exception));
    }
}
