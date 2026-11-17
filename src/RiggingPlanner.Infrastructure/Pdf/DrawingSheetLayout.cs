using RiggingPlanner.Domain;
using SkiaSharp;

namespace RiggingPlanner.Infrastructure.Pdf;

public enum DrawingSheetAlign
{
    Left,
    Center,
    Right,
}

/// <summary>One measured line of the example sheet (stroke center coordinates, points).</summary>
public readonly record struct DrawingSheetLine(double X0, double X1, double Top, double Bottom, double Width);

/// <summary>A cell (or block) of the stamp, points, top-left origin.</summary>
public readonly record struct DrawingSheetCell(double Left, double Top, double Right, double Bottom)
{
    public double Width => Right - Left;

    public double Height => Bottom - Top;

    public double CenterX => (Left + Right) / 2.0;

    public double CenterY => (Top + Bottom) / 2.0;
}

/// <summary>A text run already positioned by the layout (origin and baseline, points).</summary>
public sealed record DrawingSheetPlacedText(
    string Text,
    double Size,
    string Family,
    double AnchorX,
    double AnchorY,
    double OriginX,
    double BaselineY,
    DrawingSheetAlign Align,
    DrawingSheetCell? Cell);

public sealed record DrawingSheetContent(
    IReadOnlyList<DrawingSheetLine> Lines,
    IReadOnlyList<DrawingSheetPlacedText> Texts);

/// <summary>
/// Geometry of the drawing sheet (frame and stamp) measured from the reference example
/// (drawing-sheet.reference.json, page 2 of the example PDF). All values in points with
/// the origin at the top-left corner. The layout is also responsible for positioning the
/// texts: the renderer only draws what is listed here.
/// </summary>
public static class DrawingSheetLayout
{
    public const double PageWidth = 595.28;
    public const double PageHeight = 841.89;

    public const string LineColor = "#C0C0C0";

    // Frame
    public const double OuterLeft = 42.5;
    public const double OuterTop = 14.19;
    public const double OuterRight = 581.1;
    public const double OuterBottom = 827.79;
    public const double InnerLeft = 56.5;
    public const double InnerTop = 28.39;
    public const double InnerRight = 566.9;
    public const double InnerBottom = 813.59;
    public const double FrameLineWidth = 1.98;
    public const double ZoneMarkWidth = 0.99;
    public const double StampLineWidth = 1.42;
    public const double SignatureLineWidth = 0.51;

    // Stamp rows and columns
    public const double StampTop = 662.39;
    public const double HeaderBottom = 684.59;
    public const double DataBottom = 705.19;
    public const double LabelColumnLeft = 211.4;
    public const double LabelColumnRight = 286.2;
    public const double ValueColumnRight = 336.7;
    public const double OperationRight = 566.9;
    public const double ClientRight = 530.0;
    public const double PaperRight = 566.9;
    public const double CompanyRight = 211.4;
    public const double SheetCellRight = 566.7;

    public const double HeaderDescriptionRight = 311.6;
    public const double HeaderLengthRight = 375.4;
    public const double HeaderWidthRight = 439.1;
    public const double HeaderHeightRight = 502.9;

    public const double CompanyBlockTop = 753.39;
    public const double SignatureRowTop = 729.29;
    public const double RiggerRowTop = 741.29;
    public const double RiggerRowBottom = 753.39;
    public const double RiggerRight = 136.9;
    public const double RevisionTop = 801.59;
    public const double ScaleRight = 462.0;
    public const double ArtBottom = 783.79;

    // Text sizes
    public const double ZoneLabelSize = 15.0;
    public const double HeaderSize = 8.0;
    public const double DataValueSize = 8.0;
    public const double LabelSize = 6.0;
    public const double CornerLabelSize = 5.67;
    public const double ArtNumberSize = 9.0;
    public const double PaperSize = 11.0;
    public const double OperationValueSize = 16.0;
    public const double ClientValueSize = 16.0;
    public const double RiggerNameSize = 10.0;
    public const double RiggerLabelSize = 7.0;
    public const double CompanyNameSize = 12.0;
    public const double CompanyLineSize = 7.0;
    public const double RevisionValueSize = 6.0;

    // Small offsets measured from the example
    public const double HeaderLineOffset = 4.0;
    public const double LabelInsetX = 2.84;
    public const double ValueInsetX = 4.25;
    public const double CornerLabelInsetX = 2.84;
    public const double UnitGap = 1.6;
    public const double CompanyCenterX = 134.0;
    public const double SignatureCenterX = 133.6;
    public const double CompanyNameCenterY = 764.52;
    public const double CompanyCnpjCenterY = 776.42;
    public const double CompanyAddressCenterY = 786.25;
    public const double CompanyDistrictCenterY = 796.09;
    public const double CompanyCepCenterY = 805.92;
    public const double OperationLabelCenterY = 713.02;
    public const double ClientLabelCenterY = 771.39;
    public const double ArtLabelCenterY = 771.39;
    public const double PlanLabelCenterY = 789.76;
    public const double RevisionValueX = 239.36;
    public const double ScaleValueX = 363.67;
    public const double SheetLabelX = 467.23;
    public const double FittedTextMargin = 4.0;
    public const double FittedLineStepRatio = 1.2;

    public const string CompanyName = "Guindastes Ribas Ltda.";
    public const string CompanyCnpj = "CNPJ 81.076.242/0001-60";
    public const string CompanyAddress = "Rua Justo Manfron, 2845 G";
    public const string CompanyDistrict = "Santa Felicidade - Curitiba - PR";
    public const string CompanyCep = "CEP 82.410-540";
    public const string OperationLabel = "OPERAÇÃO:";
    public const string ClientLabel = "CLIENTE:";
    public const string ArtLabel = "ART:";
    public const string PlanLabel = "Nº Rigging:";
    public const string RevisionLabel = "Revisão:";
    public const string RevisionValue = "1 de 1";
    public const string ScaleLabel = "ESCALA";
    public const string ScaleValue = "Diversas";
    public const string PaperLabel = "A4";

    private static readonly DrawingSheetCell DescriptionHeaderCell = new(InnerLeft, StampTop, HeaderDescriptionRight, HeaderBottom);
    private static readonly DrawingSheetCell LengthHeaderCell = new(HeaderDescriptionRight, StampTop, HeaderLengthRight, HeaderBottom);
    private static readonly DrawingSheetCell WidthHeaderCell = new(HeaderLengthRight, StampTop, HeaderWidthRight, HeaderBottom);
    private static readonly DrawingSheetCell HeightHeaderCell = new(HeaderWidthRight, StampTop, HeaderHeightRight, HeaderBottom);
    private static readonly DrawingSheetCell WeightHeaderCell = new(HeaderHeightRight, StampTop, InnerRight, HeaderBottom);

    private static readonly DrawingSheetCell DescriptionDataCell = new(InnerLeft, HeaderBottom, HeaderDescriptionRight, DataBottom);
    private static readonly DrawingSheetCell LengthDataCell = new(HeaderDescriptionRight, HeaderBottom, HeaderLengthRight, DataBottom);
    private static readonly DrawingSheetCell WidthDataCell = new(HeaderLengthRight, HeaderBottom, HeaderWidthRight, DataBottom);
    private static readonly DrawingSheetCell HeightDataCell = new(HeaderWidthRight, HeaderBottom, HeaderHeightRight, DataBottom);
    private static readonly DrawingSheetCell WeightDataCell = new(HeaderHeightRight, HeaderBottom, InnerRight, DataBottom);

    private static readonly DrawingSheetCell OperationCell = new(ValueColumnRight, DataBottom, OperationRight, 765.39);
    private static readonly DrawingSheetCell ClientCell = new(ValueColumnRight, 765.39, ClientRight, RevisionTop);
    private static readonly DrawingSheetCell PaperCell = new(ClientRight, 765.39, PaperRight, RevisionTop);
    private static readonly DrawingSheetCell ArtCell = new(LabelColumnLeft, 765.39, ValueColumnRight, ArtBottom);
    private static readonly DrawingSheetCell PlanCell = new(LabelColumnLeft, ArtBottom, ValueColumnRight, RevisionTop);
    private static readonly DrawingSheetCell RevisionCell = new(LabelColumnLeft, RevisionTop, ValueColumnRight, InnerBottom);
    private static readonly DrawingSheetCell ScaleCell = new(ValueColumnRight, RevisionTop, ScaleRight, InnerBottom);
    private static readonly DrawingSheetCell SheetCell = new(ScaleRight, RevisionTop, SheetCellRight, InnerBottom);
    private static readonly DrawingSheetCell SignatureRowCell = new(InnerLeft, SignatureRowTop, CompanyRight, RiggerRowTop);
    private static readonly DrawingSheetCell RiggerCell = new(InnerLeft, RiggerRowTop, RiggerRight, RiggerRowBottom);
    private static readonly DrawingSheetCell CreaCell = new(RiggerRight, RiggerRowTop, CompanyRight, RiggerRowBottom);
    private static readonly DrawingSheetCell CompanyBlock = new(InnerLeft, CompanyBlockTop, CompanyRight, InnerBottom);

    private static readonly (double Top, double Bottom)[] LabelRows =
    [
        (DataBottom, 717.19),
        (717.19, 729.29),
        (729.29, 741.29),
        (741.29, 753.39),
        (753.39, 765.39),
    ];

    /// <summary>All measured lines of the example (frame, zone marks and stamp table).</summary>
    public static readonly IReadOnlyList<DrawingSheetLine> Lines =
    [
        new(42.5, 42.5, 14.19, 827.79, FrameLineWidth),
        new(42.5, 581.1, 14.19, 14.19, FrameLineWidth),
        new(170.0, 170.0, 14.19, 28.39, ZoneMarkWidth),
        new(311.8, 311.8, 14.19, 42.59, ZoneMarkWidth),
        new(311.8, 311.8, 14.19, 28.39, ZoneMarkWidth),
        new(453.5, 453.5, 14.19, 28.39, ZoneMarkWidth),
        new(581.1, 581.1, 14.19, 827.79, FrameLineWidth),
        new(56.5, 56.5, 28.39, 813.59, FrameLineWidth),
        new(56.5, 566.9, 28.39, 28.39, FrameLineWidth),
        new(566.7, 566.9, 28.39, 813.59, FrameLineWidth),
        new(42.5, 56.6, 137.49, 137.49, ZoneMarkWidth),
        new(566.9, 581.1, 137.49, 137.49, ZoneMarkWidth),
        new(42.5, 56.6, 279.29, 279.29, ZoneMarkWidth),
        new(566.9, 581.1, 279.29, 279.29, ZoneMarkWidth),
        new(42.5, 70.8, 420.99, 420.99, ZoneMarkWidth),
        new(42.5, 56.6, 420.99, 420.99, ZoneMarkWidth),
        new(552.7, 581.1, 420.99, 420.99, ZoneMarkWidth),
        new(566.9, 581.1, 420.99, 420.99, ZoneMarkWidth),
        new(42.5, 56.6, 562.69, 562.69, ZoneMarkWidth),
        new(566.9, 581.1, 562.69, 562.69, ZoneMarkWidth),
        new(56.5, 566.7, 662.39, 662.39, StampLineWidth),
        new(311.6, 311.6, 662.39, 704.79, StampLineWidth),
        new(375.4, 375.4, 662.39, 704.79, StampLineWidth),
        new(439.1, 439.1, 662.39, 704.79, StampLineWidth),
        new(502.9, 502.9, 662.39, 704.79, StampLineWidth),
        new(56.5, 566.7, 684.59, 684.59, StampLineWidth),
        new(42.5, 56.6, 704.49, 704.49, ZoneMarkWidth),
        new(566.9, 581.1, 704.49, 704.49, ZoneMarkWidth),
        new(56.6, 566.9, 705.19, 705.19, StampLineWidth),
        new(211.4, 211.4, 705.19, 813.59, StampLineWidth),
        new(286.2, 286.2, 705.19, 765.39, StampLineWidth),
        new(336.7, 336.7, 705.19, 813.59, StampLineWidth),
        new(211.4, 336.7, 717.19, 717.19, StampLineWidth),
        new(211.4, 336.7, 729.29, 729.29, StampLineWidth),
        new(56.6, 336.7, 741.29, 741.29, StampLineWidth),
        new(136.9, 136.9, 741.29, 753.39, StampLineWidth),
        new(56.6, 336.7, 753.39, 753.39, StampLineWidth),
        new(211.4, 566.9, 765.39, 765.39, StampLineWidth),
        new(530.0, 530.0, 765.39, 801.59, StampLineWidth),
        new(211.4, 336.7, 783.79, 783.79, StampLineWidth),
        new(211.4, 566.7, 801.59, 801.59, StampLineWidth),
        new(462.0, 462.0, 801.59, 813.59, StampLineWidth),
        new(56.5, 566.7, 813.59, 813.59, FrameLineWidth),
        new(170.0, 170.0, 813.59, 827.79, ZoneMarkWidth),
        new(311.8, 311.8, 813.59, 827.79, ZoneMarkWidth),
        new(453.5, 453.5, 813.59, 827.79, ZoneMarkWidth),
        new(42.5, 581.1, 827.79, 827.79, FrameLineWidth),
    ];

    public static readonly DrawingSheetLine SignatureLine = new(78.8, 188.4, 729.29, 729.29, SignatureLineWidth);

    private static readonly (double Left, double Right, string Label)[] ZoneNumbers =
    [
        (InnerLeft, 170.0, "4"),
        (170.0, 311.8, "3"),
        (311.8, 453.5, "2"),
        (453.5, InnerRight, "1"),
    ];

    private static readonly (double Top, double Bottom, string Label)[] ZoneLetters =
    [
        (InnerTop, 137.49, "F"),
        (137.49, 279.29, "E"),
        (279.29, 420.99, "D"),
        (420.99, 562.69, "C"),
        (562.69, 704.49, "B"),
        (704.49, InnerBottom, "A"),
    ];

    private static readonly string[] GrossWeightUnits = ["t", "t", "%", "kgf", "m/s"];

    public static DrawingSheetContent Compose(DrawingSheetStampModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var texts = new List<DrawingSheetPlacedText>();

        // Zone numbers (top and bottom) and letters (left and right), centered in their cells.
        foreach (var (left, right, label) in ZoneNumbers)
        {
            AddCentered(texts, label, ZoneLabelSize, PdfFontRegistry.GothicFamilyName, (left + right) / 2.0, (OuterTop + InnerTop) / 2.0, new DrawingSheetCell(left, OuterTop, right, InnerTop));
            AddCentered(texts, label, ZoneLabelSize, PdfFontRegistry.GothicFamilyName, (left + right) / 2.0, (InnerBottom + OuterBottom) / 2.0, new DrawingSheetCell(left, InnerBottom, right, OuterBottom));
        }

        foreach (var (top, bottom, label) in ZoneLetters)
        {
            AddCentered(texts, label, ZoneLabelSize, PdfFontRegistry.GothicFamilyName, (OuterLeft + InnerLeft) / 2.0, (top + bottom) / 2.0, new DrawingSheetCell(OuterLeft, top, InnerLeft, bottom));
            AddCentered(texts, label, ZoneLabelSize, PdfFontRegistry.GothicFamilyName, (InnerRight + OuterRight) / 2.0, (top + bottom) / 2.0, new DrawingSheetCell(InnerRight, top, OuterRight, bottom));
        }

        // Header
        AddCentered(texts, "DESCRIÇÃO DA CARGA", HeaderSize, PdfFontRegistry.GothicFamilyName, DescriptionHeaderCell.CenterX, DescriptionHeaderCell.CenterY, DescriptionHeaderCell);
        AddHeaderLabel(texts, "COMPRIMENTO", "[m]", LengthHeaderCell);
        AddHeaderLabel(texts, "LARGURA", "[m]", WidthHeaderCell);
        AddHeaderLabel(texts, "ALTURA", "[m]", HeightHeaderCell);
        AddHeaderLabel(texts, "PESO", "[kg]", WeightHeaderCell);

        // Load data row
        AddCentered(texts, model.LoadDescription, DataValueSize, PdfFontRegistry.GothicFamilyName, DescriptionDataCell.CenterX, DescriptionDataCell.CenterY, DescriptionDataCell);
        AddCentered(texts, model.LengthM, DataValueSize, PdfFontRegistry.GothicFamilyName, LengthDataCell.CenterX, LengthDataCell.CenterY, LengthDataCell);
        AddCentered(texts, model.WidthM, DataValueSize, PdfFontRegistry.GothicFamilyName, WidthDataCell.CenterX, WidthDataCell.CenterY, WidthDataCell);
        AddCentered(texts, model.HeightM, DataValueSize, PdfFontRegistry.GothicFamilyName, HeightDataCell.CenterX, HeightDataCell.CenterY, HeightDataCell);
        AddCentered(texts, model.WeightKg, DataValueSize, PdfFontRegistry.GothicFamilyName, WeightDataCell.CenterX, WeightDataCell.CenterY, WeightDataCell);

        // Summary labels and values (one per row)
        var labels = new[]
        {
            "CARGA BRUTA",
            "CAPACIDADE CARGA",
            "TAXA DE UTILIZAÇÃO",
            "ESFORÇO NAS SAPATAS",
            "VELOC. MÁX VENTO",
        };

        var values = new[]
        {
            model.GrossWeightTons,
            model.LoadCapacityT,
            model.CraneUtilizationPct,
            model.FootingForceKgf,
            model.MaxWindSpeedMs,
        };

        for (var i = 0; i < LabelRows.Length; i++)
        {
            var (top, bottom) = LabelRows[i];
            var labelCell = new DrawingSheetCell(LabelColumnLeft, top, LabelColumnRight, bottom);
            var valueCell = new DrawingSheetCell(LabelColumnRight, top, ValueColumnRight, bottom);
            var centerY = (top + bottom) / 2.0;

            AddLeft(texts, labels[i], LabelSize, PdfFontRegistry.GothicFamilyName, LabelColumnLeft + LabelInsetX, centerY, labelCell);

            var valueX = LabelColumnRight + ValueInsetX;
            AddLeft(texts, values[i], LabelSize, PdfFontRegistry.GothicFamilyName, valueX, centerY, valueCell);
            var unitX = valueX + InkWidth(values[i], LabelSize) + UnitGap;
            AddLeft(texts, GrossWeightUnits[i], LabelSize, PdfFontRegistry.GothicFamilyName, unitX, centerY, valueCell);
        }

        // Operation and client (shrink to fit one line, then wrap at the minimum size)
        AddLeft(texts, OperationLabel, CornerLabelSize, PdfFontRegistry.GothicFamilyName, ValueColumnRight + CornerLabelInsetX, OperationLabelCenterY, OperationCell);
        AddFitted(texts, model.OperationName, OperationCell, PdfFontRegistry.GothicFamilyName, OperationValueSize);

        AddLeft(texts, ClientLabel, CornerLabelSize, PdfFontRegistry.GothicFamilyName, ValueColumnRight + CornerLabelInsetX, ClientLabelCenterY, ClientCell);
        AddFitted(texts, model.ClientName, ClientCell, PdfFontRegistry.GothicFamilyName, ClientValueSize);

        AddCentered(texts, PaperLabel, PaperSize, PdfFontRegistry.GothicFamilyName, PaperCell.CenterX, PaperCell.CenterY, PaperCell);

        AddLeft(texts, ArtLabel, CornerLabelSize, PdfFontRegistry.GothicFamilyName, LabelColumnLeft + CornerLabelInsetX, ArtLabelCenterY, ArtCell);
        AddCentered(texts, model.ArtNumber, ArtNumberSize, PdfFontRegistry.GothicFamilyName, ArtCell.CenterX, ArtCell.CenterY, ArtCell);

        AddLeft(texts, PlanLabel, CornerLabelSize, PdfFontRegistry.GothicFamilyName, LabelColumnLeft + CornerLabelInsetX, PlanLabelCenterY, PlanCell);
        AddCentered(texts, model.PlanNumber, ArtNumberSize, PdfFontRegistry.GothicFamilyName, PlanCell.CenterX, PlanCell.CenterY, PlanCell);

        AddLeft(texts, RevisionLabel, CornerLabelSize, PdfFontRegistry.GothicFamilyName, LabelColumnLeft + CornerLabelInsetX, RevisionCell.CenterY, RevisionCell);
        AddLeft(texts, RevisionValue, RevisionValueSize, PdfFontRegistry.GothicFamilyName, RevisionValueX, RevisionCell.CenterY, RevisionCell);
        AddLeft(texts, ScaleLabel, CornerLabelSize, PdfFontRegistry.GothicFamilyName, ValueColumnRight + CornerLabelInsetX, ScaleCell.CenterY, ScaleCell);
        AddLeft(texts, ScaleValue, CornerLabelSize, PdfFontRegistry.GothicFamilyName, ScaleValueX, ScaleCell.CenterY, ScaleCell);
        AddLeft(texts, model.SheetLabel, CornerLabelSize, PdfFontRegistry.GothicFamilyName, SheetLabelX, SheetCell.CenterY, SheetCell);

        // Signature block
        AddCentered(texts, model.RiggerName, RiggerNameSize, PdfFontRegistry.BrandFamilyName, SignatureCenterX, SignatureRowCell.CenterY, SignatureRowCell);
        AddCentered(texts, model.RiggerLabel, RiggerLabelSize, PdfFontRegistry.GothicFamilyName, RiggerCell.CenterX, RiggerCell.CenterY, RiggerCell);
        AddCentered(texts, model.CreaLabel, RiggerLabelSize, PdfFontRegistry.GothicFamilyName, CreaCell.CenterX, CreaCell.CenterY, CreaCell);

        // Company block
        AddCentered(texts, CompanyName, CompanyNameSize, PdfFontRegistry.BrandFamilyName, CompanyCenterX, CompanyNameCenterY, CompanyBlock);
        AddCentered(texts, CompanyCnpj, CompanyLineSize, PdfFontRegistry.GothicFamilyName, CompanyCenterX, CompanyCnpjCenterY, CompanyBlock);
        AddCentered(texts, CompanyAddress, CompanyLineSize, PdfFontRegistry.GothicFamilyName, CompanyCenterX, CompanyAddressCenterY, CompanyBlock);
        AddCentered(texts, CompanyDistrict, CompanyLineSize, PdfFontRegistry.GothicFamilyName, CompanyCenterX, CompanyDistrictCenterY, CompanyBlock);
        AddCentered(texts, CompanyCep, CompanyLineSize, PdfFontRegistry.GothicFamilyName, CompanyCenterX, CompanyCepCenterY, CompanyBlock);

        return new DrawingSheetContent(Lines, texts);
    }

    private static void AddHeaderLabel(List<DrawingSheetPlacedText> texts, string label, string unit, DrawingSheetCell cell)
    {
        AddCentered(texts, label, HeaderSize, PdfFontRegistry.GothicFamilyName, cell.CenterX, cell.CenterY - HeaderLineOffset, cell);
        AddCentered(texts, unit, HeaderSize, PdfFontRegistry.GothicFamilyName, cell.CenterX, cell.CenterY + HeaderLineOffset, cell);
    }

    private static void AddFitted(List<DrawingSheetPlacedText> texts, string text, DrawingSheetCell cell, string family, double maxSize)
    {
        const double minSize = 8.0;
        const double step = 0.25;
        var maxWidth = cell.Width - 2.0 * FittedTextMargin;

        // Prefer a single line, shrinking the font down to the minimum size.
        var size = maxSize;
        for (; size > minSize; size -= step)
        {
            if (DrawingSheetTextMeasurer.WrapLines(text, size, family, maxWidth).Count == 1)
            {
                break;
            }
        }

        var lines = DrawingSheetTextMeasurer.WrapLines(text, size, family, maxWidth);
        var lineStep = FittedLineStepRatio * size;
        for (var i = 0; i < lines.Count; i++)
        {
            var y = cell.CenterY + (i - (lines.Count - 1) / 2.0) * lineStep;
            AddCentered(texts, lines[i], size, family, cell.CenterX, y, cell);
        }
    }

    private static void AddCentered(List<DrawingSheetPlacedText> texts, string text, double size, string family, double x, double y, DrawingSheetCell? cell) =>
        texts.Add(Place(text, size, family, DrawingSheetAlign.Center, x, y, cell));

    private static void AddLeft(List<DrawingSheetPlacedText> texts, string text, double size, string family, double x, double y, DrawingSheetCell? cell) =>
        texts.Add(Place(text, size, family, DrawingSheetAlign.Left, x, y, cell));

    private static DrawingSheetPlacedText Place(string text, double size, string family, DrawingSheetAlign align, double x, double y, DrawingSheetCell? cell)
    {
        var ink = DrawingSheetTextMeasurer.InkBounds(text, size, family);
        var originX = align switch
        {
            DrawingSheetAlign.Left => x - ink.Left,
            DrawingSheetAlign.Right => x - ink.Right,
            _ => x - (ink.Left + ink.Right) / 2.0,
        };

        var baselineY = y - (ink.Top + ink.Bottom) / 2.0;
        return new DrawingSheetPlacedText(text, size, family, x, y, originX, baselineY, align, cell);
    }

    private static double InkWidth(string text, double size) =>
        DrawingSheetTextMeasurer.InkBounds(text, size, PdfFontRegistry.GothicFamilyName).Width;
}
