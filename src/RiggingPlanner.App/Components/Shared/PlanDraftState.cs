using RiggingPlanner.App.Components.Plan.Attachments;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.App.Components.Shared;

public sealed class PlanDraftState
{
    public PlanFormModel Form { get; } = new();
    public RiggingPlan? Plan { get; set; }
    public List<PlanAccessory> Accessories { get; } = [];
    public AttachmentDraftState Attachments { get; } = new();
    public Guid? PlanId { get; set; }
    public bool IsInitialized { get; set; }
    public bool IsPersisted { get; set; }
    public bool SuppressLeaveConfirmation { get; set; }

    public string GetPageTitle(RiggingPlan? fallbackPlan = null)
    {
        var plan = Plan ?? fallbackPlan;
        var planNumber = plan?.PlanNumber;

        if (string.IsNullOrWhiteSpace(planNumber))
        {
            return "Novo plano";
        }

        var description = !string.IsNullOrWhiteSpace(Form.OperationName)
            ? Form.OperationName.Trim()
            : !string.IsNullOrWhiteSpace(plan?.OperationName)
                ? plan.OperationName.Trim()
                : !string.IsNullOrWhiteSpace(Form.LoadDescription)
                    ? Form.LoadDescription.Trim()
                    : plan?.LoadDescription?.Trim();

        return string.IsNullOrWhiteSpace(description)
            ? $"Plano {planNumber}"
            : $"Plano {planNumber} - {description}";
    }

    public void Reset()
    {
        Plan = null;
        PlanId = null;
        Accessories.Clear();
        Attachments.Reset();
        IsInitialized = false;
        IsPersisted = false;
        SuppressLeaveConfirmation = false;
        Form.Reset();
    }
}
