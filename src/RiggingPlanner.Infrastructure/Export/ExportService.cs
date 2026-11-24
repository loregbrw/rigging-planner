using Microsoft.EntityFrameworkCore;
using RiggingPlanner.Application.Abstractions;
using RiggingPlanner.Application.Export;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Domain;
using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Infrastructure.Persistence;
using SkiaSharp;

namespace RiggingPlanner.Infrastructure.Export;

public sealed class ExportService(
    IDbContextFactory<AppDbContext> dbFactory,
    IFileStore fileStore,
    ISummarySheetRenderer summarySheetRenderer,
    IDrawingSheetsRenderer drawingsRenderer,
    IOperationalProcedureRenderer procedureRenderer,
    FullPlanPdfAssembler pdfAssembler) : IExportService
{
    public async Task<PlanExportDocument> BuildAsync(Guid planId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var plan = await LoadPlanAsync(db, planId);

        var accessories = await db.PlanAccessories
            .AsNoTracking()
            .Where(x => x.PlanId == plan.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        var summaryModel = SummarySheetBuilder.Build(plan, accessories);
        var summaryPdf = summarySheetRenderer.Render(summaryModel);

        var drawingsPdf = await BuildDrawingsPdfAsync(plan, accessories);
        var planArt = await TryReadFileAsync(plan.ArtFile);
        var procedurePdf = procedureRenderer.Render(plan, plan.ProcedureSnapshot);
        var equipmentReport = await ResolveEquipmentReportFileAsync(db, plan);
        var reportPdf = await TryReadFileAsync(equipmentReport);

        var pdf = pdfAssembler.Assemble(summaryPdf, drawingsPdf, planArt, procedurePdf, reportPdf);
        var warnings = await BuildWarningsAsync(db, plan);

        return new PlanExportDocument(pdf, $"plano-completo-{plan.PlanNumber}.pdf", warnings);
    }

    public async Task<IReadOnlyList<string>> GetWarningsAsync(Guid planId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var plan = await LoadPlanAsync(db, planId);
        return await BuildWarningsAsync(db, plan);
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

            var bytes = await TryReadFileAsync(drawing.File);
            if (bytes is null)
            {
                continue;
            }

            var info = SKBitmap.DecodeBounds(bytes);
            if (info.Width == 0)
            {
                info = new SKImageInfo(800, 600);
            }
            imageItems.Add(new DrawingSheetImageItem(drawing.FileId, bytes, info.Width, info.Height));
        }

        if (imageItems.Count == 0)
        {
            return null;
        }

        var stampModel = DrawingSheetStampBuilder.Build(plan, accessories, 1, 1);
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

    private async Task<byte[]?> TryReadFileAsync(StoredFile? file)
    {
        if (file is null)
        {
            return null;
        }

        var absolutePath = fileStore.AbsolutePath(file.RelativePath);
        return File.Exists(absolutePath) ? await fileStore.ReadAsync(file.RelativePath) : null;
    }
}
