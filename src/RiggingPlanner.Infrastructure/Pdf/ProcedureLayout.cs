namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Layout constants and metric positions measured from pages 6 and 7 of the reference plan (2607001).
/// Origin at top-left, dimensions in points (pt), standard A4 (595.32 x 841.92).
/// </summary>
public static class ProcedureLayout
{
    public const double PageWidth = 595.32;
    public const double PageHeight = 841.92;

    // Header dimensions and coordinates
    public const double LogoBoxX1 = 43.12;
    public const double LogoBoxX2 = 146.82;
    public const double LogoBoxY1 = 27.3;
    public const double LogoBoxY2 = 85.85;

    public const double VerticalLine1X = 160.8;
    public const double VerticalLine2X = 438.55;
    public const double VerticalLineY1 = 29.02;
    public const double VerticalLineY2 = 83.97;
    public const double VerticalLineWidth = 0.75;
    public const string VerticalLineColor = "#18436E";

    public const double CenterAreaX1 = 161.0;
    public const double CenterAreaX2 = 438.0;
    public const double CenterAreaWidth = 277.0; // max ~270 pt
    public const double CenterHeaderX = 299.9;

    public const double ProcedimentoTitleTop = 36.1;
    public const double ProcedimentoTitleSize = 18.0;

    public const double OperationNameTop = 61.8;
    public const double OperationNameSize = 12.0;
    public const double OperationNameMinSize = 8.0;

    public const double RightHeaderCenterX = 501.52;
    public const double ArtLabelTop = 26.0;
    public const double ArtLabelSize = 11.04;
    public const double ArtValueTop = 43.1;
    public const double ArtValueSize = 11.04;

    public const double RightHeaderLineX1 = 445.42;
    public const double RightHeaderLineX2 = 557.62;
    public const double RightHeaderLineY1 = 56.04;
    public const double RightHeaderLineY2 = 56.52;
    public const double RightHeaderLineWidth = 0.48;
    public const string RightHeaderLineColor = "#2F5496";

    public const double RiggingLabelTop = 61.2;
    public const double RiggingLabelSize = 11.04;
    public const double RiggingValueTop = 78.4;
    public const double RiggingValueSize = 11.04;

    public const double HeaderBottomLineY1 = 91.94;
    public const double HeaderBottomLineY2 = 92.42;
    public const double HeaderBottomLineX1 = 35.4;
    public const double HeaderBottomLineSplitX = 445.41;
    public const double HeaderBottomLineX2 = 557.62;
    public const double HeaderBottomLineWidth = 0.48;
    public const string HeaderBottomLineLeftColor = "#657C9C";
    public const string HeaderBottomLineRightColor = "#2F5496";

    // Body content coordinates
    public const double BodyLeftX = 78.02;
    public const double BodyRightX = 538.7;
    public const double BulletX = 81.38;
    public const double ItemTextX = 99.26;

    public const double SectionTitleSize = 14.04;
    public const double ObjectiveLabelSize = 12.0;
    public const double ObjectiveTextSize = 12.0;
    public const double SubheadLabelSize = 12.0;
    public const double ItemBulletSize = 10.0;
    public const double ItemTextSize = 12.0;

    // Vertical rhythm deltas relative to section title top T
    public const double DeltaTitleToObjective = 27.4;
    public const double DeltaTitleToPassos = 52.3;
    public const double DeltaTitleToFirstStep = 75.4;
    public const double StepLineAdvance = 16.97;
    public const double DeltaLastStepToReferencias = 24.9;
    public const double DeltaReferenciasToFirstRef = 23.0;
    public const double RefLineAdvance = 16.97;

    // Fixed section title top positions
    public const double Section1TitleTop = 108.6;
    public const double Section2TitleTop = 357.8;
    public const double Section3TitleTop = 589.9;
    public const double Section4TitleTop = 108.6;
    public const double Section5TitleTop = 425.6;

    // Section separator line
    public const double SeparatorX1 = 78.0;
    public const double SeparatorX2 = 538.7;
    public const double SeparatorThickness = 1.55;
    public const double SeparatorDeltaAfterLastRef = 23.85;
    public const double NextTitleDeltaAfterSeparator = 15.7;
    public const string SeparatorColor = "#A0A0A0";

    // Footer
    public const double FooterLineX1 = 34.08;
    public const double FooterLineX2 = 589.92;
    public const double FooterLineY = 804.24;
    public const double FooterLineWidth = 0.48;
    public const string FooterLineColor = "#000000";

    public const double FooterTextX = 68.42;
    public const double FooterTextTop = 809.4;
    public const double FooterTextSize = 9.0;
    public const string FooterTextColor = "#4472C4";

    public const double FooterPageNumberRightX = 555.54;
    public const double FooterPageNumberTop = 807.8;
    public const double FooterPageNumberSize = 11.04;

    // Measured baselines (example plan 2607001, pages 6 and 7)
    public const double ProcedimentoTitleBaseline = 50.28;
    public const double OperationNameBaseline = 71.28;
    public const double OperationNameCenterX = 301.26;
    public const double OperationNameLineAdvance = 11.0;
    public const double ArtLabelBaseline = 34.68;
    public const double ArtValueBaseline = 51.84;
    public const double RiggingLabelBaseline = 69.96;
    public const double RiggingValueBaseline = 87.14;
    public const double FooterTextBaseline = 816.12;
    public const double FooterPageNumberBaseline = 816.12;

    public const double Section1TitleBaseline = 119.18;
    public const double Section2TitleBaseline = 368.33;
    public const double Section3TitleBaseline = 600.43;
    public const double Section4TitleBaseline = 119.18;
    public const double Section5TitleBaseline = 436.13;

    public const double DeltaTitleToObjectiveBaseline = 25.80;
    public const double DeltaTitleToPassosBaseline = 50.76;
    public const double DeltaTitleToFirstStepBaseline = 73.80;

    // Liberation Sans ascent used by QuestPDF when placing text (measured).
    public const double SansAscentRatio = 0.92166;

    // Ink ascent of 12 pt Carlito item text above the baseline (measured);
    // the separator sits SeparatorDeltaAfterLastRef below the last reference top.
    public const double ItemInkAscent = 9.0;

    // Maximum space compression per gap when an item line barely exceeds the body width.
    public const double ItemMaxSpaceCompression = 0.5;
}
