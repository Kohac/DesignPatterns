using DesignPatterns.Application.Data.DecoratorPattern.Reports;
using DesignPatterns.Application.Data.DecoratorPattern.Reports.Create;
using Microsoft.AspNetCore.Mvc;

namespace DesignPatterns.Api.Controller;

public static class DecoratorController
{
    public static WebApplication RegisterDecoratorControllers(this WebApplication app)
    {
        app.MapPost("Decorator/Reports", PostReports);
        return app;
    }

    private static async Task<IResult> PostReports([FromBody] ReportCreateRequest request, IReportStorage storage, CancellationToken cancellationToken)
    {
        await storage.Save(request, cancellationToken);
        return Results.NoContent();
    }
}
