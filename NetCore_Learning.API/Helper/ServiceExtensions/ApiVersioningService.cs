
using Asp.Versioning;

namespace NetCore_Learning.API.Helper.ServiceExtensions
{
    public static class ApiVersioningService
    {
    public static IServiceCollection AddApiVersioningService(this IServiceCollection services)
    {
        // Configure API Versioning
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0); // Default v1.0
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // Display v1, v2, ...
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
    }
}
