using DoubleCheck.Configuration;
using DoubleCheck.Reporting;
using DoubleCheck.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DoubleCheck.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDoubleCheckServices(
        this IServiceCollection services,
        AppSettings settings)
    {
        services.AddSingleton(settings);

        if (settings.UseLocalData)
            services.AddSingleton<IDataverseService>(
                _ => new LocalDataService(settings.LocalDataPath));
        else
            services.AddSingleton<IDataverseService, DataverseService>();
        services.AddSingleton<IDuplicateDetectionService, DuplicateDetectionService>();
        services.AddSingleton<IReportWriter, CsvReportWriter>();
        return services;
    }
}
