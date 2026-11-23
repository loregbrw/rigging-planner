using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.App.Components.Plan.Export;

public sealed record PlanExportResult(string FilePath, IReadOnlyList<string> Warnings);

public sealed class PlanExportService(
    IDbContextFactory<AppDbContext> dbFactory,
    ISummarySheetRenderer summarySheetRenderer,
    IDrawingSheetsRenderer drawingsRenderer,
    IOperationalProcedureRenderer procedureRenderer,
    FullPlanPdfAssembler pdfAssembler)
{
    public async Task<IReadOnlyList<string>> GetWarningsAsync(Guid planId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var plan = await LoadPlanAsync(db, planId);
        return await BuildWarningsAsync(db, plan);
    }

    public async Task<PlanExportResult> ExportAsync(Guid planId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var plan = await LoadPlanAsync(db, planId);

        var accessories = await db.PlanAccessories
            .AsNoTracking()
            .Where(x => x.PlanId == plan.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        var summaryModel = RiggingPlanner.Domain.SummarySheetBuilder.Build(plan, accessories);
        var summaryPdf = summarySheetRenderer.Render(summaryModel);

        var drawingsPdf = await BuildDrawingsPdfAsync(plan, accessories);
        var planArt = await TryReadFileAsync(plan.ArtFile);
        var procedurePdf = procedureRenderer.Render(plan, plan.ProcedureSnapshot);
        var equipmentReport = await ResolveEquipmentReportFileAsync(db, plan);
        var reportPdf = await TryReadFileAsync(equipmentReport);

        var pdf = pdfAssembler.Assemble(summaryPdf, drawingsPdf, planArt, procedurePdf, reportPdf);
        var warnings = await BuildWarningsAsync(db, plan);

        var downloads = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(downloads);
        var filePath = Path.Combine(downloads, $"plano-completo-{plan.PlanNumber}.pdf");
        await File.WriteAllBytesAsync(filePath, pdf);

        await Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(
            new Microsoft.Maui.ApplicationModel.OpenFileRequest("Plano completo", new Microsoft.Maui.Storage.ReadOnlyFile(filePath)));

        return new PlanExportResult(filePath, warnings);
    }

    private static async Task<RiggingPlan> LoadPlanAsync(AppDbContext db, Guid planId)
    {
        var plan = await db.RiggingPlans
            .AsNoTracking()
            .Include(x => x.ArtFile)
            .Include(x => x.Drawings)
                .ThenInclude(x => x.File)
            .Include(x => x.EquipmentReport)
                .ThenInclude(x => x!.File)
            .Include(x => x.ProcedureSnapshot)
            .SingleOrDefaultAsync(x => x.Id == planId && x.DeletedAt == null);

        return plan ?? throw new InvalidOperationException("Plano não encontrado.");
    }

    private async Task<byte[]?> BuildDrawingsPdfAsync(RiggingPlan plan, IReadOnlyList<PlanAccessory> accessories)
    {
        var drawings = plan.Drawings.OrderBy(x => x.SortOrder).ToList();
        if (drawings.Count == 0)
        {
            return null;
        }

        var imageItems = new List<DrawingSheetImageItem>();
        foreach (var drawing in drawings)
        {
            if (drawing.File is null)
            {
                continue;
            }

            var filePath = Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, drawing.File.RelativePath);
            if (!File.Exists(filePath))
            {
                continue;
            }

            var bytes = await File.ReadAllBytesAsync(filePath);
            var info = SkiaSharp.SKBitmap.DecodeBounds(bytes);
            if (info.Width == 0) info = new SkiaSharp.SKImageInfo(800, 600);
            imageItems.Add(new DrawingSheetImageItem(drawing.FileId, bytes, info.Width, info.Height));
        }

        if (imageItems.Count == 0)
        {
            return null;
        }

        var stampModel = RiggingPlanner.Domain.DrawingSheetStampBuilder.Build(plan, accessories, 1, 1);
        return drawingsRenderer.RenderWithImages(stampModel, imageItems);
    }

    private static async Task<StoredFile?> ResolveEquipmentReportFileAsync(AppDbContext db, RiggingPlan plan)
    {
        if (plan.EquipmentReport?.File is not null)
        {
            return plan.EquipmentReport.File;
        }

        if (plan.EquipmentId is null)
        {
            return null;
        }

        var report = await db.EquipmentReports
            .AsNoTracking()
            .Include(x => x.File)
            .Where(x => x.EquipmentId == plan.EquipmentId && (x.ValidTo == null || x.ValidTo >= plan.PlanDate))
            .OrderByDescending(x => x.ValidFrom)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        report ??= await db.EquipmentReports
            .AsNoTracking()
            .Include(x => x.File)
            .Where(x => x.EquipmentId == plan.EquipmentId)
            .OrderByDescending(x => x.ValidFrom)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        return report?.File;
    }

    private static async Task<IReadOnlyList<string>> BuildWarningsAsync(AppDbContext db, RiggingPlan plan)
    {
        var warnings = new List<string>();
        if (plan.ArtFile is null)
        {
            warnings.Add("A ART do plano não está anexada.");
        }

        var hasValidReport = plan.EquipmentReport?.File is not null
            || (plan.EquipmentId is not null && await db.EquipmentReports
                .AsNoTracking()
                .AnyAsync(x => x.EquipmentId == plan.EquipmentId
                    && (x.ValidTo == null || x.ValidTo >= plan.PlanDate)));
        if (!hasValidReport)
        {
            warnings.Add("O equipamento não tem laudo válido.");
        }

        return warnings;
    }

    private static async Task<byte[]?> TryReadFileAsync(StoredFile? file)
    {
        if (file is null)
        {
            return null;
        }

        var filePath = Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, file.RelativePath);
        return File.Exists(filePath) ? await File.ReadAllBytesAsync(filePath) : null;
    }
}
