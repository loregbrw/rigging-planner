using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Domain;

namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Renders the drawing sheet (frame, stamp and images) as an A4 PDF using QuestPDF.
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

    public byte[] RenderWithImages(DrawingSheetStampModel baseModel, IReadOnlyList<DrawingSheetImageItem> images)
    {
        ArgumentNullException.ThrowIfNull(baseModel);
        ArgumentNullException.ThrowIfNull(images);

        if (images.Count == 0)
        {
            return RenderFrameOnly(baseModel);
        }

        PdfFontRegistry.EnsureRegistered();
        var inputs = images.Select(i => new DrawingSheetImageInput(i.Id, i.WidthPx, i.HeightPx)).ToList();
        var planResult = DrawingSheetPlanner.Plan(inputs);
        var imageDict = images.ToDictionary(i => i.Id);

        return Document.Create(container =>
        {
            foreach (var pagePlan in planResult.Pages)
            {
                var stampModel = baseModel with { SheetLabel = $"FOLHA {pagePlan.SheetNumber} DE {pagePlan.SheetTotal}" };
                var content = DrawingSheetLayout.Compose(stampModel);

                container.Page(page =>
                {
                    page.Size(new PageSize((float)DrawingSheetLayout.PageWidth, (float)DrawingSheetLayout.PageHeight, Unit.Point));
                    page.Margin(0);

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

                        foreach (var imgLayout in pagePlan.Images)
                        {
                            if (imageDict.TryGetValue(imgLayout.ImageId, out var item))
                            {
                                layers.Layer().Element(c => ComposeImage(c, imgLayout, item.Data));
                            }
                        }

                        layers.Layer().Element(c => ComposeTexts(c, content.Texts));
                    });
                });
            }
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

    private static IContainer ComposeImage(IContainer container, DrawingSheetImageLayout layout, byte[] imageBytes)
    {
        container
            .PaddingTop((float)layout.Y)
            .PaddingLeft((float)layout.X)
            .Width((float)layout.Width)
            .Height((float)layout.Height)
            .Background(Colors.White)
            .Image(imageBytes);
        return container;
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
