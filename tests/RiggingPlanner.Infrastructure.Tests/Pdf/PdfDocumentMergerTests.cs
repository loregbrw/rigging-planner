using RiggingPlanner.Domain;
using RiggingPlanner.Infrastructure.Pdf;
using RiggingPlanner.Infrastructure.Tests.Fixtures;
using UglyToad.PdfPig;

namespace RiggingPlanner.Infrastructure.Tests.Pdf;

public sealed class PdfDocumentMergerTests
{
    [Fact]
    public void Merge_combines_pages_from_all_documents()
    {
        var (plan, accessories) = TestPlans.ReferencePlan();
        var model = SummarySheetBuilder.Build(plan, accessories);
        var pdf = new SummarySheetRenderer().Render(model);

        var merged = new PdfDocumentMerger().Merge([pdf, pdf]);

        using var document = PdfDocument.Open(merged);
        Assert.Equal(2, document.NumberOfPages);
    }
}
