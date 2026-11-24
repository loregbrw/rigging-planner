using RiggingPlanner.Domain.ValueObjects;

namespace RiggingPlanner.Domain.Tests;

public class PlanNumberTests
{
    [Fact]
    public void GetYearMonth_UsesThePlanDate()
    {
        Assert.Equal("2607", PlanNumber.GetYearMonth(new DateTime(2026, 7, 3)));
        Assert.Equal("2612", PlanNumber.GetYearMonth(new DateTime(2026, 12, 31)));
    }

    [Fact]
    public void Next_IncrementsTheMonthlySequence()
    {
        Assert.Equal("2607001", PlanNumber.Next("2607", 0).ToString());
        Assert.Equal("2607012", PlanNumber.Next("2607", 11).ToString());
    }
}
