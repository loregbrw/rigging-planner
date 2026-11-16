using RiggingPlanner.Domain.Entities;
using RiggingPlanner.Domain.Services;

namespace RiggingPlanner.Domain.Tests;

public class OperationalProcedureServiceTests
{
    [Theory]
    [InlineData("MD 45.007", null, "MD45007")]
    [InlineData("AT 130-1", null, "AT130-1")]
    [InlineData("MD 45.007", "Palfinger", "MD45007")]
    [InlineData("Palfinger MD 45.007", "Palfinger", "MD45007")]
    [InlineData(null, null, "-")]
    [InlineData("   ", null, "-")]
    public void DeriveCraneModelCode_ShouldFormatCorrectly(string? input, string? brand, string expected)
    {
        var result = OperationalProcedureService.DeriveCraneModelCode(input, brand);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateContent_ShouldIncludeVariablesAndSections()
    {
        var plan = new RiggingPlan
        {
            PlanNumber = "2611001",
            OperationName = "Içamento Chiller",
            ArtNumber = "0000000000000",
            CraneModel = "MD 45.007"
        };

        var content = OperationalProcedureService.GenerateContent(plan);
        var sha = OperationalProcedureService.ComputeSha256(content);

        Assert.Contains("PROCEDIMENTO", content);
        Assert.Contains("Operação: Içamento Chiller", content);
        Assert.Contains("ART: 0000000000000", content);
        Assert.Contains("RIGGING: 2611001", content);
        Assert.Contains("2. PATOLAMENTO DO GUINDASTE MD45007", content);
        Assert.False(string.IsNullOrWhiteSpace(sha));
        Assert.Equal(64, sha.Length);
    }

    [Fact]
    public void BuildDocument_ShouldSplitFixedSectionsIntoTwoPages()
    {
        var plan = new RiggingPlan
        {
            PlanNumber = "2611001",
            OperationName = "Içamento Chiller",
            ArtNumber = "0000000000000",
            CraneModel = "MD 45.007",
            CraneBrand = "Palfinger"
        };

        var document = OperationalProcedureService.BuildDocument(plan);

        Assert.Equal("MD45007", document.CraneModelCode);
        Assert.Equal(2, document.Pages.Count);
        Assert.Equal(new[] { 1, 2, 3 }, document.Pages[0].Sections.Select(x => x.Number).ToArray());
        Assert.Equal(new[] { 4, 5 }, document.Pages[1].Sections.Select(x => x.Number).ToArray());
        Assert.Equal("2. PATOLAMENTO DO GUINDASTE MD45007",
            $"2. {document.Pages[0].Sections[1].Title}");
    }

    [Fact]
    public void BuildDocument_ShouldRenderMissingValuesAsDash()
    {
        var plan = new RiggingPlan
        {
            PlanNumber = "2611001",
            OperationName = "Içamento",
            ArtNumber = null,
            CraneModel = null
        };

        var document = OperationalProcedureService.BuildDocument(plan);

        Assert.Equal("-", document.ArtNumber);
        Assert.Equal("-", document.CraneModelCode);
        Assert.Equal("2. PATOLAMENTO DO GUINDASTE -",
            $"2. {document.Pages[0].Sections[1].Title}");
    }

    [Fact]
    public void CreateSnapshot_ShouldFreezeContentCodeAndHash()
    {
        var plan = new RiggingPlan
        {
            Id = Guid.NewGuid(),
            PlanNumber = "2611001",
            OperationName = "Içamento Chiller",
            ArtNumber = "0000000000000",
            CraneModel = "MD 45.007"
        };

        var createdAt = new DateTime(2026, 11, 14, 10, 30, 0);
        var snapshot = OperationalProcedureService.CreateSnapshot(plan, createdAt);

        Assert.Equal(plan.Id, snapshot.PlanId);
        Assert.Equal("MD45007", snapshot.CraneModelCode);
        Assert.Equal(createdAt, snapshot.CreatedAt);
        Assert.Equal(OperationalProcedureService.ComputeSha256(snapshot.Content), snapshot.ContentSha256);
        Assert.Equal(64, snapshot.ContentSha256.Length);
    }

    [Fact]
    public void ResolveContent_ShouldUseSnapshotOnlyForFinalizedPlan()
    {
        var plan = new RiggingPlan
        {
            PlanNumber = "2611001",
            OperationName = "Içamento Chiller",
            ArtNumber = "0000000000000",
            CraneModel = "MD 45.007"
        };

        var snapshot = OperationalProcedureService.CreateSnapshot(plan);
        snapshot.Content = "conteúdo congelado";

        plan.Status = "draft";
        Assert.NotEqual("conteúdo congelado", OperationalProcedureService.ResolveContent(plan, snapshot));

        plan.Status = "finalized";
        Assert.Equal("conteúdo congelado", OperationalProcedureService.ResolveContent(plan, snapshot));
    }
}
