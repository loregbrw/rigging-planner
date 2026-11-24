using QuestPDF.Drawing;

namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Registers the embedded fonts (SIL OFL) with QuestPDF exactly once.
/// </summary>
public static class PdfFontRegistry
{
    public const string FamilyName = "Carlito";

    /// <summary>Substitute for Century Gothic.</summary>
    public const string GothicFamilyName = "Questrial";

    /// <summary>Substitute for Eras ITC Demi.</summary>
    public const string BrandFamilyName = "Open Sans SemiBold";

    public const string GothicFontFileName = "Questrial-Regular.ttf";
    public const string BrandFontFileName = "OpenSans-SemiBold.ttf";

    public const string SansFamilyName = "Liberation Sans";
    public const string SansRegularFileName = "LiberationSans-Regular.ttf";
    public const string SansBoldFileName = "LiberationSans-Bold.ttf";

    private static readonly object RegistrationLock = new();
    private static bool _registered;

    public static void EnsureRegistered()
    {
        if (_registered)
        {
            return;
        }

        lock (RegistrationLock)
        {
            if (_registered)
            {
                return;
            }

            // QuestPDF Community license: free of charge for projects whose annual gross revenue is below 1 million USD
            // (condition confirmed by the project owner on 16/11/2026). See https://www.questpdf.com/license/.
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            using (var regular = OpenEmbeddedFont("Carlito-Regular.ttf"))
            {
                FontManager.RegisterFontFromStream(regular);
            }

            using (var bold = OpenEmbeddedFont("Carlito-Bold.ttf"))
            {
                FontManager.RegisterFontFromStream(bold);
            }

            using (var gothic = OpenEmbeddedFont(GothicFontFileName))
            {
                FontManager.RegisterFontFromStream(gothic);
            }

            using (var brand = OpenEmbeddedFont(BrandFontFileName))
            {
                FontManager.RegisterFontFromStream(brand);
            }

            using (var sansRegular = OpenEmbeddedFont(SansRegularFileName))
            {
                FontManager.RegisterFontFromStream(sansRegular);
            }

            using (var sansBold = OpenEmbeddedFont(SansBoldFileName))
            {
                FontManager.RegisterFontFromStream(sansBold);
            }

            _registered = true;
        }
    }

    private static Stream OpenEmbeddedFont(string fileName)
    {
        var assembly = typeof(PdfFontRegistry).Assembly;
        var resourceName = $"RiggingPlanner.Infrastructure.Pdf.Fonts.{fileName}";
        var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded font resource '{resourceName}' was not found.");
        return stream;
    }
}
