using System.Globalization;

namespace RiggingPlanner.Domain.ValueObjects;

public readonly record struct PlanNumber(string YearMonth, int Sequence)
{
    public static string GetYearMonth(DateTime planDate) =>
        planDate.ToString("yyMM", CultureInfo.InvariantCulture);

    public static PlanNumber Next(string yearMonth, int lastSequence) =>
        new(yearMonth, lastSequence + 1);

    public override string ToString() => $"{YearMonth}{Sequence:000}";
}
