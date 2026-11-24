using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiggingPlanner.Application;
using RiggingPlanner.Application.Abstractions;
using RiggingPlanner.Application.Catalog;
using RiggingPlanner.Application.Export;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Application.Plans;
using RiggingPlanner.Infrastructure.Export;
using RiggingPlanner.Infrastructure.Persistence;
using RiggingPlanner.Infrastructure.Plans;
using RiggingPlanner.Infrastructure.Storage;

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
        services.AddApplication();
        services.AddSingleton<IFileStore>(new FileStore(databasePath));
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ICatalogService, Catalog.CatalogService>();
        services.AddScoped<IExportService, ExportService>();
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
