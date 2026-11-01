using DesignPatterns.Application.Data.StrategyPattern.Notifications;
using DesignPatterns.Application.Data.StrategyPattern.Notifications.Create;
using Microsoft.AspNetCore.Mvc;

namespace DesignPatterns.Api.Controller;

public static class StrategyController
{
    public static WebApplication RegisterStrategyControllers(this WebApplication app)
    {
        app.MapPost("Strategy/Notifications", PostNotifications)
            .WithDescription("Add a notification to the strategy.")
            .Produces(204)
            .ProducesValidationProblem()
            .ProducesProblem(404);
        return app;
    }

    private static async Task<IResult> PostNotifications(
        [FromBody] NotificationCreateRequest request,
        NotificationService notificationService,
        CancellationToken cancellationToken)
    {
        await notificationService.SendNotification(request.Notification, cancellationToken);
        return Results.NoContent();
    }
}
