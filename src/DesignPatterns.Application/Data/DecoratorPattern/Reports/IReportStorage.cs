using DesignPatterns.Application.Data.DecoratorPattern.Reports.Create;

namespace DesignPatterns.Application.Data.DecoratorPattern.Reports;

public interface IReportStorage
{
    Task Save(ReportCreateRequest request, CancellationToken cancellationToken);
}
