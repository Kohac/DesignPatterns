using DesignPatterns.Application.Data.DecoratorPattern.Reports.Create;

namespace DesignPatterns.Application.Data.DecoratorPattern.Reports;

public class FileReportStorage(string filePath) : IReportStorage
{
    public async Task Save(ReportCreateRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ReportCreateRequest.IsValidFileName(request.Name))
            throw new ArgumentException("A simple file name is required.", nameof(request));

        Directory.CreateDirectory(filePath);
        var path = Path.Combine(filePath, request.Name);
        await File.WriteAllBytesAsync(path, request.Content, cancellationToken);
    }
}
