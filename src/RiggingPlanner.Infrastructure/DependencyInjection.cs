using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();

        // Factory (short-lived contexts) because a Blazor Hybrid scope lives as long as the app.
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddSingleton<ISummarySheetRenderer, Pdf.SummarySheetRenderer>();
        services.AddSingleton<IDrawingSheetsRenderer, Pdf.DrawingSheetsRenderer>();
        services.AddSingleton<IOperationalProcedureRenderer, Pdf.OperationalProcedureRenderer>();
        services.AddSingleton<IPdfDocumentMerger, Pdf.PdfDocumentMerger>();
        services.AddSingleton<FullPlanPdfAssembler>();
        services.AddScoped<PlanFinalizationService>();

        return services;
    }

    public static void ApplyMigrations(this IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();

        using var context = factory.CreateDbContext();
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
}
