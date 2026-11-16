namespace RiggingPlanner.Domain.Services;

public sealed class OperationalProcedureDocument
{
    public string OperationName { get; init; } = "-";
    public string ArtNumber { get; init; } = "-";
    public string PlanNumber { get; init; } = "-";
    public string CraneModelCode { get; init; } = "-";
    public IReadOnlyList<OperationalProcedurePage> Pages { get; init; } = [];
}

public sealed class OperationalProcedurePage
{
    public int Number { get; init; }
    public IReadOnlyList<OperationalProcedureSection> Sections { get; init; } = [];
}

public sealed class OperationalProcedureSection
{
    public int Number { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Objective { get; init; } = string.Empty;
    public IReadOnlyList<string> Steps { get; init; } = [];
    public IReadOnlyList<string> References { get; init; } = [];
}
