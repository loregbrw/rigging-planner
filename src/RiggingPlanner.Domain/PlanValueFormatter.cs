using System.Globalization;

namespace RiggingPlanner.Domain;

/// <summary>
/// Formats plan values for printed documents (pt-BR, no thousands separator).
/// Rounding is display-only: values must never be rounded before being stored or combined.
/// </summary>
public static class PlanValueFormatter
{
    private static readonly CultureInfo PtBrCulture = new("pt-BR");

    /// <summary>0 to 2 decimal places, trailing zeros removed ("4,7", "1,95", "3450").</summary>
    public static string Trim2(decimal? value) =>
        value is null ? "-" : TrimTrailingZeros(Round(value.Value, 2).ToString("F2", PtBrCulture), minDecimals: 0);

    /// <summary>1 to 2 decimal places ("45,0", "5,1", "7,95").</summary>
    public static string Min1Max2(decimal? value) =>
        value is null ? "-" : TrimTrailingZeros(Round(value.Value, 2).ToString("F2", PtBrCulture), minDecimals: 1);

    /// <summary>Exactly 1 decimal place ("47,1").</summary>
    public static string Fixed1(decimal? value) =>
        value is null ? "-" : NormalizeZero(Round(value.Value, 1)).ToString("F1", PtBrCulture);

    /// <summary>Exactly 2 decimal places ("3,75").</summary>
    public static string Fixed2(decimal? value) =>
        value is null ? "-" : NormalizeZero(Round(value.Value, 2)).ToString("F2", PtBrCulture);

    /// <summary>0 decimal places ("3747").</summary>
    public static string Int0(decimal? value) =>
        value is null ? "-" : NormalizeZero(Round(value.Value, 0)).ToString("F0", PtBrCulture);

    /// <summary>Date in dd/MM/yyyy ("24/07/2026").</summary>
    public static string Date(DateTime? value) =>
        value is null ? "-" : value.Value.ToString("dd/MM/yyyy", PtBrCulture);

    /// <summary>Free text; empty, null or whitespace-only becomes "-". The text itself is never altered.</summary>
    public static string Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static decimal Round(decimal value, int decimals) =>
        Math.Round(value, decimals, MidpointRounding.AwayFromZero);

    private static decimal NormalizeZero(decimal value) =>
        value == 0m ? decimal.Zero : value;

    private static string TrimTrailingZeros(string formatted, int minDecimals)
    {
        var separator = PtBrCulture.NumberFormat.NumberDecimalSeparator;
        var decimalIndex = formatted.IndexOf(separator, StringComparison.Ordinal);
        if (decimalIndex < 0)
        {
            return formatted;
        }

        var digits = formatted[(decimalIndex + separator.Length)..].TrimEnd('0');
        var kept = Math.Max(digits.Length, minDecimals);
        return kept == 0
            ? formatted[..decimalIndex]
            : formatted[..(decimalIndex + separator.Length)] + digits.PadRight(kept, '0');
    }
}
