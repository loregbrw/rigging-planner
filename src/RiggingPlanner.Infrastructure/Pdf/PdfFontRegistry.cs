using System.Reflection;
using QuestPDF.Drawing;

namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Registers the embedded Carlito fonts (SIL OFL) with QuestPDF exactly once.
/// </summary>
public static class PdfFontRegistry
{
    public const string FamilyName = "Carlito";

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

            using var regular = OpenEmbeddedFont("Carlito-Regular.ttf");
            FontManager.RegisterFontFromStream(regular);

            using var bold = OpenEmbeddedFont("Carlito-Bold.ttf");
            FontManager.RegisterFontFromStream(bold);

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
