using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RiggingPlanner.Application;
using RiggingPlanner.Application.Abstractions;
using RiggingPlanner.Application.Catalog;
using RiggingPlanner.Application.EquipmentArts;
using RiggingPlanner.Application.Export;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Application.Plans;
using RiggingPlanner.Infrastructure.EquipmentArts;
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
        services.AddScoped<IEquipmentArtService, EquipmentArtService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<PlanFinalizationService>();

        return services;
    }

    public static void ApplyMigrations(this IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();

        using var context = factory.CreateDbContext();
        var migrations = context.Database.GetMigrations().ToList();
        var applied = context.Database.GetAppliedMigrations().ToList();
        var pending = context.Database.GetPendingMigrations().ToList();

        if (pending.Count == 0)
        {
            return;
        }

        if (applied.Except(migrations).Any() && !applied.Intersect(migrations).Any())
        {
            foreach (var migration in migrations.Where(pending.Contains))
            {
                context.Database.ExecuteSqlRaw(
                    "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES ({0}, {1})",
                    migration, "10.0.13");
            }

            return;
        }

        context.Database.Migrate();
    }
}
