using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Domain;
using RiggingPlanner.Infrastructure.Pdf;
using RiggingPlanner.Infrastructure.Tests.Fixtures;
using RiggingPlanner.Infrastructure.Tests.Pdf.Tools;
using UglyToad.PdfPig;

namespace RiggingPlanner.Infrastructure.Tests.Pdf;

/// <summary>
/// Stage-4 assembly check: the complete PDF keeps every part unchanged and places
/// the operational procedure after the plan ART and before the equipment report.
/// </summary>
public sealed class FullPlanPdfAssemblerTests
{
    [Fact]
    public void Complete_pdf_places_procedure_after_art_and_before_report()
    {
        var (plan, accessories) = TestPlans.Reference2607036();
        plan.OperationName = "Içamento Chiller";

        var summary = new SummarySheetRenderer().Render(SummarySheetBuilder.Build(plan, accessories));
        var drawings = SinglePagePdf("FOLHAS-DE-DESENHO");
        var art = SinglePagePdf("ART-DO-PLANO");
        var procedure = new OperationalProcedureRenderer().Render(plan, null);
        var report = SinglePagePdf("LAUDO-DO-EQUIPAMENTO");

        var assembler = new FullPlanPdfAssembler(new PdfDocumentMerger());
        var merged = assembler.Assemble(summary, drawings, art, procedure, report);

        using var document = PdfDocument.Open(merged);
        Assert.Equal(6, document.NumberOfPages);

        Assert.Contains("Kaefer", PageText(document, 1));
        Assert.Contains("FOLHAS-DE-DESENHO", PageText(document, 2));
        Assert.Contains("ART-DO-PLANO", PageText(document, 3));
        Assert.Contains("Procedimento", PageText(document, 4));
        Assert.Contains("1. ISOLAMENTO DA ÁREA", PageText(document, 4));
        Assert.Contains("Procedimento", PageText(document, 5));
        Assert.Contains("5. DESCARGA DA CARGA", PageText(document, 5));
        Assert.Contains("LAUDO-DO-EQUIPAMENTO", PageText(document, 6));

        var summaryAlone = PdfTextReader.ReadPages(summary)[0].Words.Select(w => w.Text).ToList();
        var summaryMerged = PdfTextReader.ReadPages(merged)[0].Words.Select(w => w.Text).ToList();
        Assert.Equal(summaryAlone, summaryMerged);
    }

    [Fact]
    public void Complete_pdf_skips_absent_parts_but_always_includes_procedure()
    {
        var (plan, accessories) = TestPlans.Reference2607036();
        plan.OperationName = "Içamento Chiller";

        var summary = new SummarySheetRenderer().Render(SummarySheetBuilder.Build(plan, accessories));
        var procedure = new OperationalProcedureRenderer().Render(plan, null);

        var assembler = new FullPlanPdfAssembler(new PdfDocumentMerger());
        var merged = assembler.Assemble(summary, null, null, procedure, null);

        using var document = PdfDocument.Open(merged);
        Assert.Equal(3, document.NumberOfPages);
        Assert.Contains("Procedimento", PageText(document, 2));
        Assert.Contains("Procedimento", PageText(document, 3));
    }

    private static string PageText(PdfDocument document, int pageNumber) =>
        string.Join(" ", document.GetPage(pageNumber).GetWords().Select(w => w.Text));

    private static byte[] SinglePagePdf(string text)
    {
        PdfFontRegistry.EnsureRegistered();
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.Content().Text(text).FontFamily(PdfFontRegistry.FamilyName).FontSize(14);
            });
        }).GeneratePdf();
    }
}
