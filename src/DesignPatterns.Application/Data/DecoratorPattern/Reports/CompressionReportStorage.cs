using System.IO.Compression;
using DesignPatterns.Application.Data.DecoratorPattern.Reports.Create;

namespace DesignPatterns.Application.Data.DecoratorPattern.Reports;

public class CompressionReportStorage(IReportStorage inner) : IReportStorage
{
    public async Task Save(ReportCreateRequest request, CancellationToken cancellationToken)
    {
        using var output = new MemoryStream();

        cancellationToken.ThrowIfCancellationRequested();
        await using (var zipStream = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            await zipStream.WriteAsync(request.Content, cancellationToken);
        }

        var enrichedRequest = request with
        {
            Name = request.Name + ".gz",
            Content = output.ToArray()
        };

        await inner.Save(enrichedRequest, cancellationToken);
    }
}
