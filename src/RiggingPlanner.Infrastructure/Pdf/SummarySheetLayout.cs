namespace RiggingPlanner.Infrastructure.Pdf;

public enum SummarySheetAlign
{
    Left,
    Center,
    Right,
}

/// <summary>
/// One absolutely positioned text run of the summary sheet.
/// X is the pen origin for Left, the column center for Center and the right edge for Right.
/// BaselineY is the text baseline in points, top-left page origin.
/// </summary>
public sealed record SummarySheetRun(
    string Text,
    double X,
    double BaselineY,
    double SizePt,
    bool Bold,
    SummarySheetAlign Align);

/// <summary>
/// How a border rectangle reacts to table overflow (extra sling/device rows).
/// </summary>
public enum SummarySheetRectKind
{
    /// <summary>Moves only with its section band (no extension).</summary>
    Static,

    /// <summary>Wall or divider of the sling table: extends down with extra sling rows.</summary>
    SlingWall,

    /// <summary>Sling table bottom border: moves down with extra sling rows.</summary>
    SlingBottom,

    /// <summary>Wall or divider of the device table: extends down with extra device rows (and shifts with sling rows).</summary>
    DeviceWall,

    /// <summary>Device table bottom border: moves down with extra device rows (and shifts with sling rows).</summary>
    DeviceBottom,

    /// <summary>Full-height frame bar (left/right): extends down so it follows the moved frame bottom border.</summary>
    FrameWall,
}

/// <summary>
/// Measured layout constants of the summary sheet ("Capacidade de carga", page 1).
/// Every number was measured against the approved document layout
/// using the letter-level reader; unit is PDF points with a top-left page origin.
/// </summary>
public static class SummarySheetLayout
{
    public const double PageWidth = 595.32;
    public const double PageHeight = 841.92;
    public const double PageCenterX = PageWidth / 2.0;
    public const double PageCenterY = PageHeight / 2.0;

    /// <summary>Vertical distance between consecutive table rows (sling and device tables).</summary>
    public const double RowStep = 15.96;

    /// <summary>Sling table first row baselines (number/value 198.14, description 197.78).</summary>
    public const double SlingRowNumberBaseline = 198.14;
    public const double SlingRowDescriptionBaseline = 197.78;

    /// <summary>First baseline of the wrapped observations text (upper half of its box).</summary>
    public const double ObservationsLeftX = 311.97;
    public const double ObservationsRightX = 543.5;
    public const double ObservationsFirstBaseline = 556.0;
    public const double ObservationsBottomLimit = 638.06;
    public const double ObservationsMinSizePt = 7.0;
    public const double ObservationsDefaultSizePt = 11.04;

    /// <summary>Logo fit box (86.4, 34.1 - 190.6, 90.0); the image keeps its aspect ratio, centered.</summary>
    public const double LogoBoxLeft = 86.4;
    public const double LogoBoxRight = 190.6;
    public const double LogoBoxTop = 34.1;
    public const double LogoBoxBottom = 90.0;

    /// <summary>Ascent ratio calibrated against the real QuestPDF rendering pipeline
    /// (probe test: baseline offset = ratio * font size, measured per weight and size;
    /// identical for regular and bold, linear across 7pt to 14pt).</summary>
    public const double RegularAscentRatio = 0.9521;
    public const double BoldAscentRatio = 0.9521;

    /// <summary>
    /// Border rectangles measured from the reference JSON, in the order of the file.
    /// Each entry is (x1, x2, y1, y2, kind) in points; rectangles are filled black.
    /// </summary>
    public static readonly (double X1, double X2, double Y1, double Y2, SummarySheetRectKind Kind)[] BorderRects =
    [
        (39.12, 41.04, 24.96, 781.90, SummarySheetRectKind.FrameWall),
        (41.04, 552.22, 24.96, 26.88, SummarySheetRectKind.Static),
        (550.30, 552.22, 26.88, 781.90, SummarySheetRectKind.FrameWall),
        (44.76, 46.68, 30.72, 96.50, SummarySheetRectKind.Static),
        (46.68, 546.58, 30.72, 32.64, SummarySheetRectKind.Static),
        (225.65, 227.57, 32.64, 96.50, SummarySheetRectKind.Static),
        (293.09, 294.05, 32.64, 62.64, SummarySheetRectKind.Static),
        (544.66, 546.58, 32.64, 96.50, SummarySheetRectKind.Static),
        (294.05, 544.66, 47.16, 48.12, SummarySheetRectKind.Static),
        (419.11, 420.07, 48.12, 62.64, SummarySheetRectKind.Static),
        (461.14, 462.10, 48.12, 62.64, SummarySheetRectKind.Static),
        (227.57, 546.58, 62.64, 64.56, SummarySheetRectKind.Static),
        (46.68, 546.58, 94.58, 96.50, SummarySheetRectKind.Static),
        (44.76, 46.68, 116.30, 150.14, SummarySheetRectKind.Static),
        (46.68, 546.58, 116.30, 118.22, SummarySheetRectKind.Static),
        (301.97, 302.93, 118.22, 148.22, SummarySheetRectKind.Static),
        (356.11, 357.07, 118.22, 148.22, SummarySheetRectKind.Static),
        (419.11, 420.07, 118.22, 148.22, SummarySheetRectKind.Static),
        (482.14, 483.10, 118.22, 148.22, SummarySheetRectKind.Static),
        (544.66, 546.58, 118.22, 150.14, SummarySheetRectKind.Static),
        (46.68, 544.66, 132.74, 133.70, SummarySheetRectKind.Static),
        (46.68, 546.58, 148.22, 150.14, SummarySheetRectKind.Static),
        (418.63, 420.55, 153.98, 251.66, SummarySheetRectKind.SlingWall),
        (420.55, 546.57, 153.98, 155.90, SummarySheetRectKind.Static),
        (544.66, 546.58, 155.91, 267.65, SummarySheetRectKind.SlingWall),
        (44.76, 46.68, 169.94, 251.66, SummarySheetRectKind.SlingWall),
        (46.68, 546.58, 169.94, 171.86, SummarySheetRectKind.Static),
        (87.26, 88.22, 171.86, 249.74, SummarySheetRectKind.SlingWall),
        (356.11, 357.07, 171.86, 249.74, SummarySheetRectKind.SlingWall),
        (482.14, 483.10, 171.86, 249.74, SummarySheetRectKind.SlingWall),
        (46.68, 418.63, 186.38, 187.34, SummarySheetRectKind.Static),
        (420.55, 544.65, 186.38, 187.34, SummarySheetRectKind.Static),
        (46.68, 418.63, 202.34, 203.30, SummarySheetRectKind.Static),
        (420.55, 544.65, 202.34, 203.30, SummarySheetRectKind.Static),
        (46.68, 418.63, 218.30, 219.26, SummarySheetRectKind.Static),
        (420.55, 544.65, 218.30, 219.26, SummarySheetRectKind.Static),
        (46.68, 418.63, 234.26, 235.22, SummarySheetRectKind.Static),
        (420.55, 544.65, 234.26, 235.22, SummarySheetRectKind.Static),
        (46.68, 546.58, 249.74, 251.66, SummarySheetRectKind.SlingBottom),
        (481.66, 483.58, 251.67, 267.65, SummarySheetRectKind.Static),
        (483.58, 546.58, 265.73, 267.65, SummarySheetRectKind.Static),
        (418.63, 420.55, 271.49, 321.29, SummarySheetRectKind.DeviceWall),
        (420.55, 546.57, 271.49, 273.41, SummarySheetRectKind.Static),
        (544.66, 546.58, 273.41, 337.25, SummarySheetRectKind.DeviceWall),
        (44.76, 46.68, 287.45, 321.29, SummarySheetRectKind.DeviceWall),
        (46.68, 546.58, 287.45, 289.37, SummarySheetRectKind.Static),
        (87.26, 88.22, 289.37, 319.37, SummarySheetRectKind.DeviceWall),
        (272.09, 273.05, 289.37, 319.37, SummarySheetRectKind.DeviceWall),
        (356.11, 357.07, 289.37, 319.37, SummarySheetRectKind.DeviceWall),
        (482.14, 483.10, 289.37, 319.37, SummarySheetRectKind.DeviceWall),
        (46.68, 418.63, 303.89, 304.85, SummarySheetRectKind.Static),
        (420.55, 544.65, 303.89, 304.85, SummarySheetRectKind.Static),
        (46.68, 546.58, 319.37, 321.29, SummarySheetRectKind.DeviceBottom),
        (481.66, 483.58, 321.29, 337.25, SummarySheetRectKind.Static),
        (483.58, 546.58, 335.33, 337.25, SummarySheetRectKind.Static),
        (45.72, 545.74, 346.85, 348.77, SummarySheetRectKind.Static),
        (45.72, 545.74, 381.53, 382.49, SummarySheetRectKind.Static),
        (45.72, 545.74, 396.05, 397.01, SummarySheetRectKind.Static),
        (45.72, 545.74, 410.57, 411.53, SummarySheetRectKind.Static),
        (45.72, 545.74, 415.85, 417.77, SummarySheetRectKind.Static),
        (45.72, 293.69, 454.03, 454.99, SummarySheetRectKind.Static),
        (308.93, 545.74, 454.03, 454.99, SummarySheetRectKind.Static),
        (45.72, 293.69, 469.99, 470.95, SummarySheetRectKind.Static),
        (308.93, 545.74, 469.99, 470.95, SummarySheetRectKind.Static),
        (45.72, 293.69, 485.95, 486.91, SummarySheetRectKind.Static),
        (308.93, 545.74, 485.95, 486.91, SummarySheetRectKind.Static),
        (45.72, 293.69, 501.91, 502.87, SummarySheetRectKind.Static),
        (308.93, 545.74, 501.91, 502.87, SummarySheetRectKind.Static),
        (45.72, 293.69, 517.87, 518.83, SummarySheetRectKind.Static),
        (308.93, 545.74, 517.87, 518.83, SummarySheetRectKind.Static),
        (171.26, 172.22, 518.83, 548.59, SummarySheetRectKind.Static),
        (309.89, 546.58, 531.91, 533.83, SummarySheetRectKind.Static),
        (307.97, 309.89, 531.92, 744.22, SummarySheetRectKind.Static),
        (544.66, 546.58, 533.84, 744.22, SummarySheetRectKind.Static),
        (45.72, 293.69, 547.63, 548.59, SummarySheetRectKind.Static),
        (45.72, 293.69, 592.63, 593.59, SummarySheetRectKind.Static),
        (45.72, 293.69, 609.31, 610.27, SummarySheetRectKind.Static),
        (45.72, 293.69, 625.30, 626.26, SummarySheetRectKind.Static),
        (45.72, 293.69, 641.26, 642.22, SummarySheetRectKind.Static),
        (45.72, 293.69, 657.22, 658.18, SummarySheetRectKind.Static),
        (45.72, 293.69, 673.18, 674.14, SummarySheetRectKind.Static),
        (45.72, 293.69, 689.14, 690.10, SummarySheetRectKind.Static),
        (45.72, 293.69, 705.10, 706.06, SummarySheetRectKind.Static),
        (45.72, 293.69, 721.06, 722.02, SummarySheetRectKind.Static),
        (45.72, 293.69, 737.02, 737.98, SummarySheetRectKind.Static),
        (309.89, 546.58, 742.30, 744.22, SummarySheetRectKind.Static),
        (41.04, 552.22, 779.98, 781.90, SummarySheetRectKind.Static),
    ];

    /// <summary>Signature line rectangles: [x1, x2, y1, y2] (from the example document).</summary>
    public static readonly double[][] SignatureLineRects =
    [
        [49.48, 279.01, 758.30, 759.10],
        [308.69, 537.62, 758.30, 759.10],
    ];

    /// <summary>Fixed text runs (labels, headers, titles, units): left-aligned at the measured pen origin.</summary>
    public static readonly SummarySheetRun[] FixedRuns =
    [
        R("CLIENTE:", 259.49, 40.68, 9.00, false),
        R("LOCAL:", 265.85, 56.64, 9.00, false),
        R("DATA:", 429.67, 58.20, 9.00, false),
        R("DADOS DA CARGA:", 48.00, 111.86, 14.04, true),
        R("Descrição da carga", 132.62, 129.62, 11.04, false),
        R("Comp. (m)", 306.29, 129.62, 11.04, false),
        R("Largura (m)", 362.59, 129.62, 11.04, false),
        R("Altura (m)", 428.71, 129.62, 11.04, false),
        R("Peso (kg)", 494.14, 129.62, 11.04, false),
        R("LINGADA / AMARRAS", 48.00, 165.50, 14.04, true),
        R("Pesos", 468.82, 165.50, 12.00, true),
        R("Item", 56.76, 182.18, 11.04, false),
        R("Descrição", 200.81, 182.18, 11.04, false),
        R("Qtd. (pç)", 368.83, 182.18, 11.04, false),
        R("Unitário (kg)", 423.55, 182.18, 11.04, false),
        R("Total (kg)", 493.42, 182.18, 11.04, false),
        R("DISPOSITIVOS / BALANCIM", 48.00, 283.01, 14.04, true),
        R("Pesos", 468.82, 283.01, 12.00, true),
        R("Item", 56.76, 300.77, 11.04, false),
        R("Descrição", 158.78, 300.77, 11.04, false),
        R("Ref. /Desenho", 283.01, 300.77, 11.04, false),
        R("Qtd. (pç)", 368.83, 300.77, 11.04, false),
        R("Unitário (kg)", 423.55, 300.77, 11.04, false),
        R("Total (kg)", 493.42, 300.77, 11.04, false),
        R("INFORMAÇÕES DO GUINDASTE", 48.00, 361.97, 14.04, true),
        R("Modelo", 69.26, 378.41, 11.04, false),
        R("Caminhão", 61.82, 392.57, 11.04, false),
        R("Placas", 410.83, 392.57, 11.04, false),
        R("Chassi", 78.98, 407.09, 11.04, false),
        R("Ano de Fabricação", 353.59, 407.09, 11.04, false),
        R("CONFIGURAÇÃO DO GUINDASTE", 88.70, 434.81, 12.00, true),
        R("COM", 342.79, 434.81, 12.00, true),
        R("POSIÇÃO DA CARGA BRUTA", 367.74, 434.81, 12.00, true),
        R("Capacidade nominal", 120.98, 449.59, 11.04, false),
        R("Carga líquida", 422.59, 450.91, 11.04, false),
        R("t", 253.49, 450.91, 11.04, true),
        R("kg", 526.54, 450.91, 11.04, true),
        R("Comprimento da lança", 109.70, 466.87, 11.04, false),
        R("m", 253.49, 466.87, 11.04, true),
        R("Lingada / Amarração", 387.55, 466.87, 11.04, false),
        R("kg", 526.54, 466.87, 11.04, true),
        R("Raio de operação", 133.70, 482.83, 11.04, false),
        R("m", 253.49, 482.83, 11.04, true),
        R("Dispositivos / Balancim", 376.75, 482.83, 11.04, false),
        R("kg", 526.54, 482.83, 11.04, true),
        R("Capacidade de carga", 119.54, 498.79, 11.04, false),
        R("t", 253.49, 498.79, 11.04, true),
        R("Cargas extras", 420.91, 498.79, 11.04, false),
        R("kg", 526.54, 498.79, 11.04, true),
        R("Carga bruta", 428.35, 514.75, 11.04, false),
        R("kg", 526.54, 514.75, 11.04, true),
        R("QUADRANTE OPERACIONAL", 100.10, 514.63, 12.00, true),
        R("Inicial", 96.98, 528.31, 9.96, true),
        R("Final", 223.25, 528.31, 9.96, true),
        R("Observações / Anexos:", 310.73, 542.83, 9.96, true),
        R("RESUMO DOS CÁLCULOS", 108.50, 574.87, 12.00, true),
        R("Total da carga bruta", 121.82, 589.51, 11.04, false),
        R("t", 253.49, 589.51, 11.04, true),
        R("Capacidade de carga do guindaste", 59.06, 606.19, 11.04, false),
        R("t", 253.49, 606.19, 11.04, true),
        R("Taxa de utilização do guindaste", 71.78, 622.18, 11.04, false),
        R("%", 253.49, 622.18, 11.04, true),
        R("Taxa de utilização da cinta plana", 67.10, 638.14, 11.04, false),
        R("%", 253.49, 638.14, 11.04, true),
        R("Taxa de utilização da cinta tubular", 59.06, 654.10, 11.04, false),
        R("%", 253.49, 654.10, 11.04, true),
        R("Taxa de utiliz. laço de cabo de aço", 59.66, 670.06, 11.04, false),
        R("%", 253.49, 670.06, 11.04, true),
        R("Taxa de utilização da manilha", 80.18, 686.02, 11.04, false),
        R("%", 253.49, 686.02, 11.04, true),
        R("Esforço nas sapatas", 124.10, 701.98, 11.04, false),
        R("kgf", 253.49, 701.98, 11.04, true),
        R("kgf/cm", 253.49, 717.82, 11.04, true),
        R("2", 286.01, 712.30, 7.32, true),
        R("Resistência mínima do terreno", 74.54, 717.94, 11.04, false),
        R("Velocidade máxima do vento", 81.38, 733.90, 11.04, false),
        R("m/s", 253.37, 733.90, 9.96, true),
        R("Supervisor Responsável", 367.39, 770.71, 11.04, false),
    ];

    // Variable run anchors (pen origin / column center / right edge), measured from the example.
    public const double TitleCenterX = 373.26;
    public const double TitleBaselineY = 84.50;
    public const double TitleSizePt = 15.96;

    public const double ClientNameX = 340.39;
    public const double ClientNameBaselineY = 44.04;
    public const double LocationX = 339.43;
    public const double LocationBaselineY = 59.04;
    public const double PlanDateX = 477.22;
    public const double PlanDateBaselineY = 59.04;

    public const double LoadDescriptionX = 47.64;
    public const double LoadDescriptionBaselineY = 144.62;
    public const double LoadValueBaselineY = 144.50;
    public const double LengthCenterX = 330.04;
    public const double WidthCenterX = 388.55;
    public const double HeightCenterX = 451.63;
    public const double WeightCenterX = 514.56;

    public const double SlingNumberCenterX = 67.16;
    public const double SlingDescriptionX = 89.54;
    public const double SlingQuantityCenterX = 388.58;
    public const double SlingUnitCenterX = 451.63;
    public const double SlingTotalCenterX = 514.60;

    public const double DeviceNumberCenterX = 67.16;
    public const double DeviceDescriptionCenterX = 180.53;
    public const double DeviceRefCenterX = 315.06;
    public const double DeviceQuantityCenterX = 388.58;
    public const double DeviceUnitCenterX = 451.63;
    public const double DeviceTotalCenterX = 514.60;

    /// <summary>
    /// Totals rows ("PESO TOTAL ...:") baselines, measured from the example, relative to
    /// each table's totals base (slingTotalsBase / deviceTotalsBase computed by the renderer).
    /// </summary>
    public const double SlingTotalsLabelOffset = 0.15;
    public const double SlingTotalsValueOffset = 0.03;
    public const double DeviceTotalsLabelOffset = 0.00;
    public const double DeviceTotalsValueOffset = -0.12;

    /// <summary>Both totals labels start at this pen origin (left of the label).</summary>
    public const double TotalsLabelLeftX = 288.05;
    public const double TotalsLabelSizePt = 11.04;

    /// <summary>Totals label texts. The device label is right-aligned so it ends where the
    /// sling label ends.</summary>
    public const string SlingTotalsLabelText = "PESO TOTAL DA LINGADA / AMARRAÇÃO:";
    public const string DeviceTotalsLabelText = "PESO TOTAL DOS DISPOSITIVOS / BALANCIM:";

    /// <summary>Top edge (original) of each table's bottom border; extra row separators are
    /// inserted at this Y + i * RowStep, thin (0.96 pt), spanning the item columns only.</summary>
    public const double SlingTableBottomY = 249.74;
    public const double DeviceTableBottomY = 319.37;
    public const double RowSeparatorThickness = 0.96;
    public static readonly (double X1, double X2)[] RowSeparatorSpans = [(46.68, 418.63), (420.55, 544.65)];

    public const double CraneValueLeftX = 110.66;
    public const double CraneModelBaselineY = 378.41;
    public const double TruckModelBaselineY = 392.57;
    public const double LicensePlateX = 442.54;
    public const double ChassisBaselineY = 407.09;
    public const double ManufactureYearX = 442.54;

    public const double ConfigLeftColumnCenterX = 233.10;
    public const double ConfigRightColumnCenterX = 504.06;
    public const double ConfigNominalCapacityBaselineY = 449.83;
    public const double ConfigNetWeightBaselineY = 449.83;
    public const double ConfigBoomLengthBaselineY = 465.79;
    public const double ConfigSlingWeightBaselineY = 465.79;
    public const double ConfigOperatingRadiusBaselineY = 481.75;
    public const double ConfigDeviceWeightBaselineY = 481.75;
    public const double ConfigLoadCapacityBaselineY = 497.71;
    public const double ConfigExtraLoadBaselineY = 497.71;
    public const double ConfigGrossWeightBaselineY = 513.67;

    public const double QuadrantInitialCenterX = 109.21;
    public const double QuadrantFinalCenterX = 233.08;
    public const double QuadrantValueBaselineY = 544.51;

    public const double ResumoValueRightX = 249.59;
    public static readonly double[] ResumoValueBaselinesY =
    [
        589.15, 604.75, 621.10, 637.06, 653.02, 668.98, 684.94, 700.90, 716.86, 732.82,
    ];

    public const double ArtCenterX = 427.28;
    public const double ArtBaselineY = 650.02;
    public const double ArtSizePt = 14.04;

    public const double RiggerSignatureCenterX = 159.38;
    public const double RiggerSignatureBaselineY = 771.38;

    /// <summary>Baseline offsets of the table texts below the sling table, relative to the base layout (0 extra rows).</summary>
    public const double DeviceSectionBaselineY = 315.77;
    public const double DeviceHeaderBaselineY = 300.77;
    public const double DeviceTitleBaselineY = 283.01;

    private static SummarySheetRun R(string text, double x, double baselineY, double sizePt, bool bold) =>
        new(text, x, baselineY, sizePt, bold, SummarySheetAlign.Left);
}
