using System.Reflection;

namespace RiggingPlanner.Infrastructure.Pdf;

/// <summary>
/// Measures Carlito text widths (points) with the same shaping engine QuestPDF uses
/// (SkParagraph), so positioned values land exactly where the reference layout expects.
/// QuestPDF 2026.9 hides its Skia text stack (internal types), so this helper reaches it
/// through reflection; the surface used here is stable within the pinned QuestPDF version.
/// </summary>
public static class CarlitoMeasurer
{
    private const string FamilyName = PdfFontRegistry.FamilyName;

    private static readonly object Lock = new();
    private static bool _initialized;

    private static Type _skTextType = null!;
    private static Type _fontCollectionType = null!;
    private static Type _textStyleType = null!;
    private static Type _configurationType = null!;
    private static Type _fontWeightsType = null!;
    private static Type _fontFeatureType = null!;
    private static Type _paragraphStyleType = null!;
    private static Type _builderType = null!;
    private static Type _paragraphType = null!;

    private static object _fontCollection = null!;
    private static object _builder = null!;
    private static Dictionary<(double Size, bool Bold, string Family), object> _styleCache = null!;
    private static MethodInfo _builderAddText = null!;
    private static MethodInfo _builderCreateParagraph = null!;
    private static MethodInfo _paragraphPlanLayout = null!;
    private static MethodInfo _paragraphGetSize = null!;
    private static MethodInfo _paragraphGetLineExtents = null!;
    private static IntPtr _familyNamePtr;
    private static IntPtr _sansFamilyNamePtr;

    public static double MeasureWidth(string text, double sizePt, bool bold, string family = FamilyName)
    {
        EnsureInitialized();

        // The shared SkParagraph builder is a native resource: concurrent calls
        // (parallel tests, UI threads) crash with an access violation.
        lock (Lock)
        {
            var style = GetStyle(sizePt, bold, family);
            var builder = _builder;
            _builderAddText.Invoke(builder, [text, style]);
            using var paragraphHandle = CreateParagraph(builder);
            var paragraph = paragraphHandle.Instance;
            _paragraphPlanLayout.Invoke(paragraph, [100000f]);
            var width = GetSize(paragraph).Width;
            ResetBuilder(builder);
            return width;
        }
    }

    /// <summary>Number of wrapped lines and total height of a paragraph constrained to maxLayoutWidth.</summary>
    public static (int LineCount, double TotalHeight) MeasureWrapped(string text, double sizePt, bool bold, double maxLayoutWidth, string family = FamilyName)
    {
        EnsureInitialized();

        lock (Lock)
        {
            var style = GetStyle(sizePt, bold, family);
            var builder = _builder;
            _builderAddText.Invoke(builder, [text, style]);
            using var paragraphHandle = CreateParagraph(builder);
            var paragraph = paragraphHandle.Instance;
            _paragraphPlanLayout.Invoke(paragraph, [(float)maxLayoutWidth]);
            var (_, height) = GetSize(paragraph);
            var lineExtents = (Array)_paragraphGetLineExtents.Invoke(paragraph, [])!;
            ResetBuilder(builder);
            return (lineExtents.Length, height);
        }
    }

    private static void ResetBuilder(object builder)
    {
        // Mirrors SkParagraphBuilderPoolManager.Return: builders are reused, not disposed
        // (disposing the builder would also dispose the shared font collection).
        builder.GetType().InvokeMember("Reset", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, builder, []);
    }

    private static object GetStyle(double sizePt, bool bold, string family)
    {
        lock (Lock)
        {
            var key = (sizePt, bold, family);
            if (!_styleCache.TryGetValue(key, out var style))
            {
                style = CreateTextStyle(sizePt, bold, family);
                _styleCache[key] = style;
            }

            return style;
        }
    }

    private static void EnsureInitialized()
    {
        lock (Lock)
        {
            if (_initialized)
            {
                return;
            }

            // Fonts must be registered in the QuestPDF FontManager first (shared typeface provider).
            PdfFontRegistry.EnsureRegistered();

            var assembly = typeof(QuestPDF.Settings).Assembly;
            var fontManagerType = assembly.GetType("QuestPDF.Drawing.FontManager", throwOnError: true)!;
            _skTextType = assembly.GetType("QuestPDF.Skia.SkText", throwOnError: true)!;
            _fontCollectionType = assembly.GetType("QuestPDF.Skia.Text.SkFontCollection", throwOnError: true)!;
            _textStyleType = assembly.GetType("QuestPDF.Skia.Text.SkTextStyle", throwOnError: true)!;
            _configurationType = assembly.GetType("QuestPDF.Skia.Text.TextStyleConfiguration", throwOnError: true)!;
            _fontWeightsType = assembly.GetType("QuestPDF.Skia.Text.TextStyleConfiguration+FontWeights", throwOnError: true)!;
            _fontFeatureType = assembly.GetType("QuestPDF.Skia.Text.TextStyleConfiguration+FontFeature", throwOnError: true)!;
            _paragraphStyleType = assembly.GetType("QuestPDF.Skia.Text.ParagraphStyle", throwOnError: true)!;
            _builderType = assembly.GetType("QuestPDF.Skia.Text.SkParagraphBuilder", throwOnError: true)!;
            _paragraphType = assembly.GetType("QuestPDF.Skia.Text.SkParagraph", throwOnError: true)!;

            _builderAddText = _builderType.GetMethods()
                .First(m => m.Name == "AddText" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(string))
                ?? throw new MissingMethodException("QuestPDF.Skia.Text.SkParagraphBuilder", "AddText");
            _builderCreateParagraph = _builderType.GetMethod("CreateParagraph")
                ?? throw new MissingMethodException("QuestPDF.Skia.Text.SkParagraphBuilder", "CreateParagraph");
            _paragraphPlanLayout = _paragraphType.GetMethod("PlanLayout")
                ?? throw new MissingMethodException("QuestPDF.Skia.Text.SkParagraph", "PlanLayout");
            _paragraphGetSize = _paragraphType.GetMethod("GetSize")
                ?? throw new MissingMethodException("QuestPDF.Skia.Text.SkParagraph", "GetSize");
            _paragraphGetLineExtents = _paragraphType.GetMethod("GetLineExtents")
                ?? throw new MissingMethodException("QuestPDF.Skia.Text.SkParagraph", "GetLineExtents");

            // Mirrors SkParagraphBuilderPoolManager.Get: the collection is built from the shared
            // FontManager typeface provider; system fonts stay out (UseSystemFonts disabled).
            var provider = fontManagerType.GetProperty("TypefaceProvider", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
            var systemFontManager = fontManagerType.GetProperty("SystemFontManager", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null);
            _fontCollection = _fontCollectionType.InvokeMember(
                "Create",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                null,
                null,
                [provider, systemFontManager])!;

            var marshal = _skTextType.GetMethod("MarshalFromManagedToNative")
                ?? throw new MissingMethodException("QuestPDF.Skia.SkText", "MarshalFromManagedToNative");
            _familyNamePtr = (IntPtr)(marshal.Invoke(null, [FamilyName])
                ?? throw new InvalidOperationException("Failed to marshal the Carlito family name."));
            _sansFamilyNamePtr = (IntPtr)(marshal.Invoke(null, [PdfFontRegistry.SansFamilyName])
                ?? throw new InvalidOperationException("Failed to marshal the Liberation Sans family name."));

            _builder = _builderType.InvokeMember(
                "Create",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                null,
                null,
                [Activator.CreateInstance(_paragraphStyleType), _fontCollection])!;

            _styleCache = new Dictionary<(double, bool, string), object>();
            _initialized = true;
        }
    }

    private static object CreateTextStyle(double sizePt, bool bold, string family)
    {
        var configuration = Activator.CreateInstance(_configurationType)!;
        _configurationType.InvokeMember("FontSize", BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField, null, configuration, [(float)sizePt]);
        _configurationType.InvokeMember("FontWeight", BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField, null, configuration, [Enum.Parse(_fontWeightsType, bold ? "Bold" : "Normal")]);

        // Both arrays are marshalled as ByValArray with fixed capacities (128 families, 32 features).
        var families = new IntPtr[128];
        families[0] = family == PdfFontRegistry.SansFamilyName ? _sansFamilyNamePtr : _familyNamePtr;
        _configurationType.InvokeMember("FontFamilies", BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField, null, configuration, [families]);

        // Must match the renderer: the reference example is shaped without kerning/ligatures.
        var features = Array.CreateInstance(_fontFeatureType, 32);
        SetFeature(features, 0, "kern", 0);
        SetFeature(features, 1, "liga", 0);
        _configurationType.InvokeMember("FontFeatures", BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField, null, configuration, [features]);

        return Activator.CreateInstance(_textStyleType, configuration)!;
    }

    private static void SetFeature(Array features, int index, string name, int value)
    {
        var feature = Activator.CreateInstance(_fontFeatureType)!;
        _fontFeatureType.InvokeMember("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField, null, feature, [name]);
        _fontFeatureType.InvokeMember("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField, null, feature, [value]);
        features.SetValue(feature, index);
    }

    private static ParagraphHandle CreateParagraph(object builder) =>
        new(_builderCreateParagraph.Invoke(builder, [])!);

    private static (float Width, float Height) GetSize(object paragraph) =>
        ((float, float))_paragraphGetSize.Invoke(paragraph, [])!;

    private readonly struct ParagraphHandle(object instance) : IDisposable
    {
        public object Instance { get; } = instance;

        public void Dispose() => ((IDisposable)Instance).Dispose();
    }
}
