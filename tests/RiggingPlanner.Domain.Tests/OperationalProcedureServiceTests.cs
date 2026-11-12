using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Domain.Services;

namespace RiggingPlanner.Domain.Tests;

public class OperationalProcedureServiceTests
{
    [Theory]
    [InlineData("MD 45.007", "MD45007")]
    [InlineData("AT 130-1", "AT130-1")]
    [InlineData(null, "-")]
    [InlineData("   ", "-")]
    public void DeriveCraneModelCode_ShouldFormatCorrectly(string? input, string expected)
    {
        var result = OperationalProcedureService.DeriveCraneModelCode(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateContent_ShouldIncludeVariablesAndSections()
    {
        var plan = new RiggingPlan
        {
            PlanNumber = "2611001",
            OperationName = "Içamento Chiller",
            ArtNumber = "1720264496217",
            CraneModel = "MD 45.007"
        };

        var content = OperationalProcedureService.GenerateContent(plan);
        var sha = OperationalProcedureService.ComputeSha256(content);

        Assert.Contains("PROCEDIMENTO OPERACIONAL", content);
        Assert.Contains("Operação: Içamento Chiller", content);
        Assert.Contains("ART: 1720264496217", content);
        Assert.Contains("RIGGING: 2611001", content);
        Assert.Contains("2. PATOLAMENTO DO GUINDASTE MD45007", content);
        Assert.False(string.IsNullOrWhiteSpace(sha));
        Assert.Equal(64, sha.Length);
    }
}
