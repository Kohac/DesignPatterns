using DesignPatterns.Application.Data.DecoratorPattern.Reports.Create;
using Microsoft.Extensions.Logging;

namespace DesignPatterns.Application.Data.DecoratorPattern.Reports;

public class LoggingReportStorage(IReportStorage inner, ILogger<LoggingReportStorage> logger) : IReportStorage
{
    public async Task Save(ReportCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Request started.");
        try
        {
            await inner.Save(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Request cancelled.");
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Request failed.");
            throw;
        }

        logger.LogInformation("Request completed.");
    }
}
