using System.Buffers.Binary;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RiggingPlanner.Application.Pdf;
using RiggingPlanner.Domain;

namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Renders the summary sheet (page 1) as a standalone PDF using QuestPDF.
/// Every text run and border rectangle is absolutely positioned with the constants
/// measured from the reference example (SummarySheetLayout). When the accessory tables
/// do not fit the fixed layout, the whole page is scaled down (nothing is lost or truncated).
/// </summary>
public sealed class SummarySheetRenderer : ISummarySheetRenderer
{
    private const string LogoResourceName = "RiggingPlanner.Infrastructure.Pdf.Images.guindastes-ribas-logo.png";
    private const string BorderColor = "#000000";

    // Sling table capacity: 4 item rows; device table: 1 item row (reference layout).
    private const int MaxSlingRows = 4;
    private const int MaxDeviceRows = 1;

    private static readonly double[] ObservationsSizeCandidates =
    [
        11.04, 10.51, 9.98, 9.45, 8.92, 8.39, 7.86, 7.32, 7.00,
    ];

    public byte[] Render(SummarySheetModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        PdfFontRegistry.EnsureRegistered();

        var extraSlingRows = Math.Max(0, model.SlingLines.Count - MaxSlingRows);
        var extraDeviceRows = Math.Max(0, model.DeviceLines.Count - MaxDeviceRows);
        var slingShift = extraSlingRows * SummarySheetLayout.RowStep;
        var deviceShift = slingShift + extraDeviceRows * SummarySheetLayout.RowStep;
        var scale = SummarySheetLayout.PageHeight / (SummarySheetLayout.PageHeight + deviceShift);

        // The shifted content spans [0, PageHeight + deviceShift]; scaling around its own
        // center (not the page center) centers the reduced sheet on the page for any overflow.
        var contentCenterY = (SummarySheetLayout.PageHeight + deviceShift) / 2;

        var rects = BuildRects(model.SlingLines.Count, model.DeviceLines.Count, slingShift, deviceShift, scale, contentCenterY);
        var runs = BuildRuns(model, slingShift, deviceShift, extraDeviceRows, scale, contentCenterY);

        var logo = LoadLogo();
        var observations = model.Observations.Trim();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(new PageSize((float)SummarySheetLayout.PageWidth, (float)SummarySheetLayout.PageHeight, Unit.Point));
                page.Margin(0);

                // The reference example was produced by Word WITHOUT OpenType kerning or
                // ligatures; Skia applies both by default, which shifts glyphs by up to
                // ~3pt on long runs. Disable them so advances match the example exactly.
                page.DefaultTextStyle(text => text
                    .FontFamily(PdfFontRegistry.FamilyName)
                    .DisableFontFeature(FontFeatures.Kerning)
                    .DisableFontFeature(FontFeatures.StandardLigatures));

                page.Content().Layers(layers =>
                {
                    // The first layer must be the primary layer (defines the layer stack size).
                    layers.PrimaryLayer().Element(c => ComposeRect(c, rects[0]));
                    foreach (var rect in rects.Skip(1))
                    {
                        layers.Layer().Element(c => ComposeRect(c, rect));
                    }

                    layers.Layer().Element(c => ComposeLogo(c, logo, scale, contentCenterY));
                    layers.Layer().Element(c => ComposeRuns(c, runs));
                    layers.Layer().Element(c => ComposeObservations(c, observations, deviceShift, scale, contentCenterY));
                });
            });
        }).GeneratePdf();
    }

    private static List<double[]> BuildRects(
        int slingCount,
        int deviceCount,
        double slingShift,
        double deviceShift,
        double scale,
        double contentCenterY)
    {
        var rects = new List<double[]>();
        var extraSlingRows = Math.Max(0, slingCount - MaxSlingRows);
        var extraDeviceRows = Math.Max(0, deviceCount - MaxDeviceRows);

        foreach (var (x1, x2, y1, y2, kind) in SummarySheetLayout.BorderRects)
        {
            double rectY1 = y1, rectY2 = y2;

            switch (kind)
            {
                case SummarySheetRectKind.SlingWall:
                    rectY2 += slingShift;
                    break;
                case SummarySheetRectKind.SlingBottom:
                    rectY1 += slingShift;
                    rectY2 += slingShift;
                    break;
                case SummarySheetRectKind.DeviceWall:
                    rectY2 += extraDeviceRows * SummarySheetLayout.RowStep;
                    break;
                case SummarySheetRectKind.DeviceBottom:
                    rectY1 += extraDeviceRows * SummarySheetLayout.RowStep;
                    rectY2 += extraDeviceRows * SummarySheetLayout.RowStep;
                    break;
                case SummarySheetRectKind.FrameWall:
                    rectY2 += deviceShift;
                    break;
            }

            // Section shift by the ORIGINAL top edge: above the sling table = static,
            // device section shifts with the sling table, crane section and below with both.
            var shift = y1 <= 250 ? 0 : y1 <= 320.5 ? slingShift : deviceShift;
            rects.Add(Transform(x1, x2, rectY1 + shift, rectY2 + shift, scale, contentCenterY));
        }

        // Additional row separators for the accessory tables that exceed the fixed layout.
        // Thin (0.96 pt) like the measured row separators, spanning the item columns only.
        for (var i = 0; i < extraSlingRows; i++)
        {
            var y = SummarySheetLayout.SlingTableBottomY + i * SummarySheetLayout.RowStep;
            foreach (var (spanX1, spanX2) in SummarySheetLayout.RowSeparatorSpans)
            {
                rects.Add(Transform(spanX1, spanX2, y, y + SummarySheetLayout.RowSeparatorThickness, scale, contentCenterY));
            }
        }

        for (var i = 0; i < extraDeviceRows; i++)
        {
            var y = SummarySheetLayout.DeviceTableBottomY + slingShift + i * SummarySheetLayout.RowStep;
            foreach (var (spanX1, spanX2) in SummarySheetLayout.RowSeparatorSpans)
            {
                rects.Add(Transform(spanX1, spanX2, y, y + SummarySheetLayout.RowSeparatorThickness, scale, contentCenterY));
            }
        }

        foreach (var source in SummarySheetLayout.SignatureLineRects)
        {
            rects.Add(Transform(source[0], source[1], source[2] + deviceShift, source[3] + deviceShift, scale, contentCenterY));
        }

        return rects;
    }

    private static double[] Transform(double x1, double x2, double y1, double y2, double scale, double contentCenterY) =>
        [
            SummarySheetLayout.PageCenterX + (x1 - SummarySheetLayout.PageCenterX) * scale,
            SummarySheetLayout.PageCenterX + (x2 - SummarySheetLayout.PageCenterX) * scale,
            SummarySheetLayout.PageCenterY + (y1 - contentCenterY) * scale,
            SummarySheetLayout.PageCenterY + (y2 - contentCenterY) * scale,
        ];

    private static List<SummarySheetRun> BuildRuns(
        SummarySheetModel model,
        double slingShift,
        double deviceShift,
        int extraDeviceRows,
        double scale,
        double contentCenterY)
    {
        var runs = new List<SummarySheetRun>();
        var slingCount = model.SlingLines.Count;
        var deviceCount = model.DeviceLines.Count;
        var slingTotalsBase = SummarySheetLayout.SlingRowNumberBaseline + slingCount * SummarySheetLayout.RowStep;
        var deviceTotalsBase = SummarySheetLayout.DeviceSectionBaselineY + slingShift + deviceCount * SummarySheetLayout.RowStep;

        void Add(string text, double x, double baseline, double size, bool bold, SummarySheetAlign align) =>
            runs.Add(new SummarySheetRun(
                text,
                SummarySheetLayout.PageCenterX + (x - SummarySheetLayout.PageCenterX) * scale,
                SummarySheetLayout.PageCenterY + (baseline - contentCenterY) * scale,
                size * scale,
                bold,
                align));

        // Header block
        Add($"PLANO DE RIGGING nº {model.PlanNumber}", SummarySheetLayout.TitleCenterX, SummarySheetLayout.TitleBaselineY, SummarySheetLayout.TitleSizePt, true, SummarySheetAlign.Center);
        Add(model.ClientName, SummarySheetLayout.ClientNameX, SummarySheetLayout.ClientNameBaselineY, 11.04, false, SummarySheetAlign.Left);
        Add(model.Location, SummarySheetLayout.LocationX, SummarySheetLayout.LocationBaselineY, 11.04, false, SummarySheetAlign.Left);
        Add(model.PlanDate, SummarySheetLayout.PlanDateX, SummarySheetLayout.PlanDateBaselineY, 11.04, false, SummarySheetAlign.Left);

        // Load table
        Add(model.LoadDescription, SummarySheetLayout.LoadDescriptionX, SummarySheetLayout.LoadDescriptionBaselineY, 11.04, false, SummarySheetAlign.Left);
        Add(model.LengthM, SummarySheetLayout.LengthCenterX, SummarySheetLayout.LoadValueBaselineY, 11.04, false, SummarySheetAlign.Center);
        Add(model.WidthM, SummarySheetLayout.WidthCenterX, SummarySheetLayout.LoadValueBaselineY, 11.04, false, SummarySheetAlign.Center);
        Add(model.HeightM, SummarySheetLayout.HeightCenterX, SummarySheetLayout.LoadValueBaselineY, 11.04, false, SummarySheetAlign.Center);
        Add(model.WeightKg, SummarySheetLayout.WeightCenterX, SummarySheetLayout.LoadValueBaselineY, 11.04, false, SummarySheetAlign.Center);

        // Sling table rows
        foreach (var (line, index) in model.SlingLines.Select((line, index) => (line, index)))
        {
            var numberBaseline = SummarySheetLayout.SlingRowNumberBaseline + index * SummarySheetLayout.RowStep;
            var descriptionBaseline = SummarySheetLayout.SlingRowDescriptionBaseline + index * SummarySheetLayout.RowStep;
            Add(line.Number.ToString(), SummarySheetLayout.SlingNumberCenterX, numberBaseline, 11.04, false, SummarySheetAlign.Center);
            Add(line.Description, SummarySheetLayout.SlingDescriptionX, descriptionBaseline, 9.96, false, SummarySheetAlign.Left);
            Add(line.Quantity, SummarySheetLayout.SlingQuantityCenterX, numberBaseline, 11.04, false, SummarySheetAlign.Center);
            Add(line.UnitWeightKg, SummarySheetLayout.SlingUnitCenterX, numberBaseline, 11.04, false, SummarySheetAlign.Center);
            Add(line.LineTotal, SummarySheetLayout.SlingTotalCenterX, numberBaseline, 11.04, false, SummarySheetAlign.Center);
        }

        Add(model.SlingTotal, SummarySheetLayout.SlingTotalCenterX, slingTotalsBase + SummarySheetLayout.SlingTotalsValueOffset, 11.04, false, SummarySheetAlign.Center);
        Add(SummarySheetLayout.SlingTotalsLabelText, SummarySheetLayout.TotalsLabelLeftX, slingTotalsBase + SummarySheetLayout.SlingTotalsLabelOffset, SummarySheetLayout.TotalsLabelSizePt, true, SummarySheetAlign.Left);

        // Device table: title and headers come from FixedRuns (they shift with slingShift).

        foreach (var (line, index) in model.DeviceLines.Select((line, index) => (line, index)))
        {
            var baseline = SummarySheetLayout.DeviceSectionBaselineY + slingShift + index * SummarySheetLayout.RowStep;
            Add(line.Number.ToString(), SummarySheetLayout.DeviceNumberCenterX, baseline, 11.04, false, SummarySheetAlign.Center);
            Add(line.Description, SummarySheetLayout.DeviceDescriptionCenterX, baseline, 11.04, false, SummarySheetAlign.Center);
            Add(line.DrawingRef, SummarySheetLayout.DeviceRefCenterX, baseline, 11.04, false, SummarySheetAlign.Center);
            Add(line.Quantity, SummarySheetLayout.DeviceQuantityCenterX, baseline, 11.04, false, SummarySheetAlign.Center);
            Add(line.UnitWeightKg, SummarySheetLayout.DeviceUnitCenterX, baseline, 11.04, false, SummarySheetAlign.Center);
            Add(line.LineTotal, SummarySheetLayout.DeviceTotalCenterX, baseline, 11.04, false, SummarySheetAlign.Center);
        }

        Add(model.DeviceTotal, SummarySheetLayout.DeviceTotalCenterX, deviceTotalsBase + SummarySheetLayout.DeviceTotalsValueOffset, 11.04, false, SummarySheetAlign.Center);

        // Intentional difference #1 (stage-1 doc): the device totals label has its own text,
        // right-aligned so it ends where the sling totals label ends in the example.
        var slingLabelWidth = CarlitoMeasurer.MeasureWidth(SummarySheetLayout.SlingTotalsLabelText, SummarySheetLayout.TotalsLabelSizePt, bold: true);
        Add(SummarySheetLayout.DeviceTotalsLabelText, SummarySheetLayout.TotalsLabelLeftX + slingLabelWidth, deviceTotalsBase + SummarySheetLayout.DeviceTotalsLabelOffset, SummarySheetLayout.TotalsLabelSizePt, true, SummarySheetAlign.Right);

        // Crane information (shifted when the accessory tables exceed the fixed layout)
        Add(model.CraneModel, SummarySheetLayout.CraneValueLeftX, SummarySheetLayout.CraneModelBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Left);
        Add(model.TruckModel, SummarySheetLayout.CraneValueLeftX, SummarySheetLayout.TruckModelBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Left);
        Add(model.LicensePlate, SummarySheetLayout.LicensePlateX, SummarySheetLayout.TruckModelBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Left);
        Add(model.ChassisNumber, SummarySheetLayout.CraneValueLeftX, SummarySheetLayout.ChassisBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Left);
        Add(model.ManufactureYear, SummarySheetLayout.ManufactureYearX, SummarySheetLayout.ChassisBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Left);

        // Crane configuration (shifted when the accessory tables exceed the fixed layout)
        Add(model.CraneNominalCapacity, SummarySheetLayout.ConfigLeftColumnCenterX, SummarySheetLayout.ConfigNominalCapacityBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);
        Add(model.NetWeightKg, SummarySheetLayout.ConfigRightColumnCenterX, SummarySheetLayout.ConfigNetWeightBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);
        Add(model.BoomLengthM, SummarySheetLayout.ConfigLeftColumnCenterX, SummarySheetLayout.ConfigBoomLengthBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);
        Add(model.SlingWeightKg, SummarySheetLayout.ConfigRightColumnCenterX, SummarySheetLayout.ConfigSlingWeightBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);
        Add(model.OperatingRadiusM, SummarySheetLayout.ConfigLeftColumnCenterX, SummarySheetLayout.ConfigOperatingRadiusBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);
        Add(model.DeviceWeightKg, SummarySheetLayout.ConfigRightColumnCenterX, SummarySheetLayout.ConfigDeviceWeightBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);
        Add(model.LoadCapacityT, SummarySheetLayout.ConfigLeftColumnCenterX, SummarySheetLayout.ConfigLoadCapacityBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);
        Add(model.ExtraLoadKg, SummarySheetLayout.ConfigRightColumnCenterX, SummarySheetLayout.ConfigExtraLoadBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);
        Add(model.GrossWeightKg, SummarySheetLayout.ConfigRightColumnCenterX, SummarySheetLayout.ConfigGrossWeightBaselineY + deviceShift, 11.04, true, SummarySheetAlign.Center);

        Add(model.QuadrantStart, SummarySheetLayout.QuadrantInitialCenterX, SummarySheetLayout.QuadrantValueBaselineY + deviceShift, 11.04, false, SummarySheetAlign.Center);
        Add(model.QuadrantEnd, SummarySheetLayout.QuadrantFinalCenterX, SummarySheetLayout.QuadrantValueBaselineY + deviceShift, 11.04, false, SummarySheetAlign.Center);

        // Summary rows
        var resumoValues = new[]
        {
            model.GrossWeightTons,
            model.CraneLoadCapacityT,
            model.CraneUtilizationPct,
            model.FlatStrapUtilizationPct,
            model.TubularStrapUtilizationPct,
            model.WireRopeSlingUtilizationPct,
            model.ShackleUtilizationPct,
            model.FootingForceKgf,
            model.MinGroundResistanceKgfCm2,
            model.MaxWindSpeedMs,
        };

        for (var i = 0; i < resumoValues.Length; i++)
        {
            Add(resumoValues[i], SummarySheetLayout.ResumoValueRightX, SummarySheetLayout.ResumoValueBaselinesY[i] + deviceShift, 11.04, true, SummarySheetAlign.Right);
        }

        Add(model.ArtDisplay, SummarySheetLayout.ArtCenterX, SummarySheetLayout.ArtBaselineY + deviceShift, SummarySheetLayout.ArtSizePt, true, SummarySheetAlign.Center);
        Add(model.RiggerSignature, SummarySheetLayout.RiggerSignatureCenterX, SummarySheetLayout.RiggerSignatureBaselineY + deviceShift, 11.04, false, SummarySheetAlign.Center);

        // Fixed labels, titles and units. Both thresholds test the ORIGINAL baseline:
        // >283 = device section (shifts with the sling table), >346 = crane section and below
        // (also shifts when the device table grows).
        foreach (var fixedRun in SummarySheetLayout.FixedRuns)
        {
            var baseline = fixedRun.BaselineY;
            if (fixedRun.BaselineY > 283.0)
            {
                baseline += slingShift;
            }

            if (fixedRun.BaselineY > 346.0)
            {
                baseline += extraDeviceRows * SummarySheetLayout.RowStep;
            }

            Add(fixedRun.Text, fixedRun.X, baseline, fixedRun.SizePt, fixedRun.Bold, SummarySheetAlign.Left);
        }

        return runs;
    }

    private static void ComposeRuns(IContainer container, List<SummarySheetRun> runs)
    {
        container.Layers(layers =>
        {
            // The first layer must be the primary layer (defines the layer stack size).
            layers.PrimaryLayer().Element(c => ComposeRun(c, runs[0]));
            foreach (var run in runs.Skip(1))
            {
                layers.Layer().Element(c => ComposeRun(c, run));
            }
        });
    }

    private static IContainer ComposeRun(IContainer container, SummarySheetRun run)
    {
        var ascent = Ascent(run.SizePt, run.Bold);
        var yTop = run.BaselineY - ascent;

        var x = run.Align switch
        {
            SummarySheetAlign.Left => run.X,
            SummarySheetAlign.Center => run.X - CarlitoMeasurer.MeasureWidth(run.Text, run.SizePt, run.Bold) / 2,
            SummarySheetAlign.Right => run.X - CarlitoMeasurer.MeasureWidth(run.Text, run.SizePt, run.Bold),
            _ => throw new ArgumentOutOfRangeException(nameof(run), run.Align, null),
        };

        var result = container
            .PaddingTop((float)yTop)
            .PaddingLeft((float)x)
            .Unconstrained();

        result.Text(text =>
        {
            var span = text.Span(run.Text).FontSize((float)run.SizePt);
            if (run.Bold)
            {
                span.Bold();
            }
        });

        return result;
    }

    private static IContainer ComposeRect(IContainer container, double[] rect)
    {
        var result = container
            .PaddingTop((float)rect[2])
            .PaddingLeft((float)rect[0])
            .Width((float)(rect[1] - rect[0]))
            .Height((float)(rect[3] - rect[2]));
        result.Background(BorderColor);
        return result;
    }

    private static IContainer ComposeLogo(IContainer container, byte[] logo, double scale, double contentCenterY)
    {
        var (imageWidth, imageHeight) = PngSize(logo);
        var boxWidth = (SummarySheetLayout.LogoBoxRight - SummarySheetLayout.LogoBoxLeft) * scale;
        var boxHeight = (SummarySheetLayout.LogoBoxBottom - SummarySheetLayout.LogoBoxTop) * scale;
        var ratio = (double)imageWidth / imageHeight;

        var drawWidth = boxWidth;
        var drawHeight = drawWidth / ratio;
        if (drawHeight > boxHeight)
        {
            drawHeight = boxHeight;
            drawWidth = drawHeight * ratio;
        }

        var left = SummarySheetLayout.PageCenterX
            + (SummarySheetLayout.LogoBoxLeft - SummarySheetLayout.PageCenterX) * scale
            + (boxWidth - drawWidth) / 2;
        var top = SummarySheetLayout.PageCenterY
            + (SummarySheetLayout.LogoBoxTop - contentCenterY) * scale
            + (boxHeight - drawHeight) / 2;

        var logoContainer = container
            .PaddingLeft((float)left)
            .PaddingTop((float)top)
            .Width((float)drawWidth)
            .Height((float)drawHeight);
        logoContainer.Image(logo).FitWidth();
        return logoContainer;
    }

    private static IContainer ComposeObservations(
        IContainer container,
        string observations,
        double deviceShift,
        double scale,
        double contentCenterY)
    {
        if (observations.Length == 0)
        {
            return container;
        }

        var left = SummarySheetLayout.PageCenterX + (SummarySheetLayout.ObservationsLeftX - SummarySheetLayout.PageCenterX) * scale;
        var width = (SummarySheetLayout.ObservationsRightX - SummarySheetLayout.ObservationsLeftX) * scale;
        var firstBaseline = SummarySheetLayout.PageCenterY
            + (SummarySheetLayout.ObservationsFirstBaseline + deviceShift - contentCenterY) * scale;
        var bottomLimit = SummarySheetLayout.PageCenterY
            + (SummarySheetLayout.ObservationsBottomLimit + deviceShift - contentCenterY) * scale;

        foreach (var size in ObservationsSizeCandidates)
        {
            var renderedSize = size * scale;
            var (lineCount, totalHeight) = CarlitoMeasurer.MeasureWrapped(observations, renderedSize, bold: false, width);
            var top = firstBaseline - Ascent(renderedSize, bold: false);
            if (top + totalHeight <= bottomLimit)
            {
                var fitted = container
                    .PaddingLeft((float)left)
                    .PaddingTop((float)top)
                    .Width((float)width);
                fitted.Text(text => text.Span(observations).FontSize((float)renderedSize));
                return fitted;
            }
        }

        var smallest = ObservationsSizeCandidates[^1] * scale;
        var smallestTop = firstBaseline - Ascent(smallest, bold: false);
        var overflow = container
            .PaddingLeft((float)left)
            .PaddingTop((float)smallestTop)
            .Width((float)width);
        overflow.Text(text => text.Span(observations).FontSize((float)smallest));
        return overflow;
    }

    private static double Ascent(double sizePt, bool bold) =>
        (bold ? SummarySheetLayout.BoldAscentRatio : SummarySheetLayout.RegularAscentRatio) * sizePt;

    private static (int Width, int Height) PngSize(byte[] png) => (
        BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)),
        BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));

    private static byte[] LoadLogo()
    {
        var assembly = typeof(SummarySheetRenderer).Assembly;
        using var stream = assembly.GetManifestResourceStream(LogoResourceName)
            ?? throw new InvalidOperationException($"Embedded logo resource '{LogoResourceName}' was not found.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
