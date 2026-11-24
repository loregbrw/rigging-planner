namespace RiggingPlanner.Application.Catalog;

public static class CnpjFormatter
{
    public static string? Format(string? value)
    {
        var normalized = Normalize(value);
        return normalized?.Length == 14
            ? $"{normalized[..2]}.{normalized[2..5]}.{normalized[5..8]}/{normalized[8..12]}-{normalized[12..]}"
            : normalized;
    }

    public static string? Normalize(string? value)
    {
        var normalized = new string((value ?? string.Empty).ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        var prefix = new string(normalized.Take(12).Where(char.IsLetterOrDigit).ToArray());
        var suffix = new string(normalized.Skip(12).Where(char.IsDigit).Take(2).ToArray());
        var digits = prefix + suffix;
        return digits.Length == 0 ? null : digits;
    }

    public static bool IsValid(string? value)
    {
        var normalized = new string((value ?? string.Empty).ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        if (normalized.Length != 14) return false;
        return normalized.Take(12).All(char.IsLetterOrDigit)
            && normalized.Skip(12).All(char.IsDigit);
    }
}
