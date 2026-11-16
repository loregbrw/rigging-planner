using QuestPDF.Fluent;
using QuestPDF.Helpers;
using RiggingPlanner.Infrastructure.Pdf;
using UglyToad.PdfPig;
using Xunit;

namespace RiggingPlanner.Infrastructure.Tests.Pdf;

public class PdfFontRegistryTests
{
    [Fact]
    public void A4Page_WithCarlitoRegularAndBold_EmbedsFontsAndPreservesAccentedText()
    {
        const string text = "Íçamento — AÇÃO 7/8";

        PdfFontRegistry.EnsureRegistered();

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.Content().Column(column =>
                {
                    column.Item().Text(text)
                        .FontFamily(PdfFontRegistry.FamilyName).FontSize(12);
                    column.Item().Text(text)
                        .FontFamily(PdfFontRegistry.FamilyName).FontSize(12).Bold();
                });
            });
        }).GeneratePdf();

        Assert.NotEmpty(pdfBytes);

        // TrueType fonts embedded as font programs carry this descriptor key.
        Assert.Contains("FontFile2"u8, pdfBytes);

        using var document = PdfDocument.Open(pdfBytes);
        var page = document.GetPage(1);
        var letters = page.Letters;

        Assert.All(letters, letter => Assert.Contains(PdfFontRegistry.FamilyName, letter.FontName));

        var extracted = new string(letters.Select(l => l.Value[0]).ToArray());
        Assert.Equal(text + text, extracted);
    }
}
