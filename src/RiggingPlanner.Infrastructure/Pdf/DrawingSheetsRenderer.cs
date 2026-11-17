using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Domain;

namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Renders the drawing sheet (frame and stamp) as a standalone A4 PDF using QuestPDF.
/// Stage 2A renders a single sheet without images; the geometry comes from
/// DrawingSheetLayout (measured from the reference example).
/// </summary>
public sealed class DrawingSheetsRenderer : IDrawingSheetsRenderer
{
    public byte[] RenderFrameOnly(DrawingSheetStampModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        PdfFontRegistry.EnsureRegistered();
        var content = DrawingSheetLayout.Compose(model);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(new PageSize((float)DrawingSheetLayout.PageWidth, (float)DrawingSheetLayout.PageHeight, Unit.Point));
                page.Margin(0);

                // The example was produced by Word WITHOUT kerning or ligatures (stage-1 finding).
                page.DefaultTextStyle(text => text
                    .FontFamily(PdfFontRegistry.GothicFamilyName)
                    .DisableFontFeature(FontFeatures.Kerning)
                    .DisableFontFeature(FontFeatures.StandardLigatures));

                page.Content().Layers(layers =>
                {
                    layers.PrimaryLayer().Element(c => ComposeRect(c, ToRect(content.Lines[0])));
                    foreach (var line in content.Lines.Skip(1))
                    {
                        layers.Layer().Element(c => ComposeRect(c, ToRect(line)));
                    }

                    layers.Layer().Element(c => ComposeRect(c, ToRect(DrawingSheetLayout.SignatureLine)));
                    layers.Layer().Element(c => ComposeTexts(c, content.Texts));
                });
            });
        }).GeneratePdf();
    }

    private static void ComposeTexts(IContainer container, IReadOnlyList<DrawingSheetPlacedText> texts)
    {
        if (texts.Count == 0)
        {
            return;
        }

        container.Layers(layers =>
        {
            layers.PrimaryLayer().Element(c => ComposeText(c, texts[0]));
            foreach (var run in texts.Skip(1))
            {
                layers.Layer().Element(c => ComposeText(c, run));
            }
        });
    }

    private static IContainer ComposeText(IContainer container, DrawingSheetPlacedText run)
    {
        var ascent = DrawingSheetTextMeasurer.AscentRatio(run.Family) * run.Size;
        var top = run.BaselineY - ascent;

        var result = container
            .PaddingTop((float)top)
            .PaddingLeft((float)run.OriginX)
            .Unconstrained();

        result.Text(text => text.Span(run.Text).FontSize((float)run.Size).FontFamily(run.Family));
        return result;
    }

    private static IContainer ComposeRect(IContainer container, double[] rect)
    {
        var result = container
            .PaddingTop((float)rect[2])
            .PaddingLeft((float)rect[0])
            .Width((float)(rect[1] - rect[0]))
            .Height((float)(rect[3] - rect[2]));
        result.Background(DrawingSheetLayout.LineColor);
        return result;
    }

    // A stroked line becomes a filled rectangle. Butt caps are the PDF default, so the
    // line does not extend beyond its endpoints; only the thickness grows by half a
    // width on each side of the stroke center.
    private static double[] ToRect(DrawingSheetLine line)
    {
        var half = line.Width / 2.0;
        if (line.Top == line.Bottom)
        {
            return [line.X0, line.X1, line.Top - half, line.Top + half];
        }

        if (line.X0 == line.X1)
        {
            return [line.X0 - half, line.X0 + half, line.Top, line.Bottom];
        }

        return [line.X0 - half, line.X1 + half, line.Top - half, line.Bottom + half];
    }
}
