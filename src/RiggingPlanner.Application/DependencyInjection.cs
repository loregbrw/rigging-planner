using Microsoft.Extensions.DependencyInjection;
using RiggingPlanner.Application.Pdf;

namespace RiggingPlanner.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<FullPlanPdfAssembler>();
        return services;
    }
}
