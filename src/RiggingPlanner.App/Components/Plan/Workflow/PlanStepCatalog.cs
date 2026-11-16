namespace RiggingPlanner.App.Components.Plan.Workflow;

public static class PlanStepCatalog
{
    public const int Entry = 0;
    public const int Capacity = 1;
    public const int Attachments = 2;
    public const int Preview = 3;
    public const int Export = 4;

    public static readonly IReadOnlyList<string> Steps =
    [
        "Dados de entrada",
        "Capacidade de Carga",
        "Anexos",
        "Preview",
        "Exportação"
    ];

    public static int GetIndex(string? segment) => string.IsNullOrWhiteSpace(segment)
        ? Attachments
        : segment.ToLowerInvariant() switch
        {
            "capacity" => Capacity,
            "attachments" => Attachments,
            "preview" => Preview,
            "export" => Export,
            _ => Attachments
        };

    public static string GetTitle(string? segment) => string.IsNullOrWhiteSpace(segment)
        ? "Etapa do plano"
        : segment.ToLowerInvariant() switch
        {
            "capacity" => "Capacidade de Carga",
            "attachments" => "Anexos",
            "preview" => "Preview",
            "export" => "Exportação",
            _ => "Etapa do plano"
        };

    public static IReadOnlyList<string> GetLinks(Guid workflowPlanId, bool isNewEntry)
    {
        var entry = isNewEntry ? "new-plan" : $"new-plan/{workflowPlanId}";
        return
        [
            entry,
            $"new-plan/{workflowPlanId}/capacity",
            $"new-plan/{workflowPlanId}/attachments",
            $"new-plan/{workflowPlanId}/preview",
            $"new-plan/{workflowPlanId}/export"
        ];
    }

    public static string? GetPreviousUrl(int step, Guid workflowPlanId) => step switch
    {
        Capacity => $"new-plan/{workflowPlanId}",
        Attachments => $"new-plan/{workflowPlanId}/capacity",
        Preview => $"new-plan/{workflowPlanId}/attachments",
        Export => $"new-plan/{workflowPlanId}/preview",
        _ => null
    };

    public static string? GetNextUrl(int step, Guid workflowPlanId) => step switch
    {
        Entry => $"new-plan/{workflowPlanId}/capacity",
        Capacity => $"new-plan/{workflowPlanId}/attachments",
        Attachments => $"new-plan/{workflowPlanId}/preview",
        Preview => $"new-plan/{workflowPlanId}/export",
        _ => null
    };
}
