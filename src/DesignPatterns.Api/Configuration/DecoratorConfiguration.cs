using DesignPatterns.Application.Data.DecoratorPattern.Reports;

namespace DesignPatterns.Api.Configuration;

public static class DecoratorConfiguration
{
    public static IServiceCollection AddDecoratorConfiguration(this IServiceCollection services, string directory)
    {
        services.AddScoped<IReportStorage>(sp =>
        {
            var storage = new FileReportStorage(directory);
            var gzipStorage = new CompressionReportStorage(storage);

            return new LoggingReportStorage(gzipStorage, sp.GetRequiredService<ILogger<LoggingReportStorage>>());
        });

        return services;
    }
}
