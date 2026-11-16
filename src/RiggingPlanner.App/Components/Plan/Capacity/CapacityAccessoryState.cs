using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.App.Components.Plan.Capacity;

public sealed class CapacityAccessoryState
{
    private readonly CapacityState parent;
    private readonly string errorTarget;

    internal CapacityAccessoryState(CapacityState parent, string section, string errorTarget, bool showDrawingRef)
    {
        this.parent = parent;
        Section = section;
        this.errorTarget = errorTarget;
        ShowDrawingRef = showDrawingRef;
    }

    public string Section { get; }
    public bool ShowDrawingRef { get; }
    public PlanAccessoryForm Form { get; private set; } = new();
    public bool IsAdding { get; private set; }
    public PlanAccessory? PendingRemoval { get; private set; }
    public bool IsEditing => Form.EditingId is not null;
    public List<PlanAccessory> Items => parent.Items.Where(x => x.Section == Section).ToList();
    public decimal Total => Items.Sum(x => x.Quantity * x.UnitWeightKg);
    public int StartIndex => Section == "dispositivo" ? parent.Lingada.Items.Count + 1 : 1;

    public void StartAdd()
    {
        IsAdding = true;
        Form = new PlanAccessoryForm();
        PendingRemoval = null;
        parent.ErrorMessage = null;
        parent.NotifyChanged();
    }

    public void Edit(PlanAccessory item)
    {
        IsAdding = false;
        Form = new PlanAccessoryForm
        {
            EditingId = item.Id,
            Description = item.Description,
            DrawingRef = item.DrawingRef,
            Quantity = item.Quantity,
            UnitWeightKg = item.UnitWeightKg
        };
        PendingRemoval = null;
        parent.ErrorMessage = null;
        parent.NotifyChanged();
    }

    public void Cancel()
    {
        IsAdding = false;
        Form = new PlanAccessoryForm();
        parent.ErrorMessage = null;
        parent.NotifyChanged();
    }

    public void Save()
    {
        if (parent.Plan is null || parent.IsReadOnly) return;

        if (string.IsNullOrWhiteSpace(Form.Description) || Form.Quantity is not > 0 || Form.UnitWeightKg is null)
        {
            parent.ErrorMessage = $"Preencha a descrição, quantidade e peso unitário válidos para {errorTarget}.";
            parent.NotifyChanged();
            return;
        }

        if (Form.EditingId is null)
        {
            var newItem = new PlanAccessory
            {
                Id = Guid.NewGuid(),
                PlanId = parent.PlanId,
                Section = Section,
                Description = Form.Description.Trim(),
                DrawingRef = ShowDrawingRef ? NormalizeDrawingRef(Form.DrawingRef) : null,
                Quantity = Form.Quantity.Value,
                UnitWeightKg = Form.UnitWeightKg.Value,
                SortOrder = parent.NextSortOrder
            };
            parent.Items.Add(newItem);
        }
        else
        {
            var existing = parent.Items.FirstOrDefault(x => x.Id == Form.EditingId);
            if (existing is not null)
            {
                existing.Description = Form.Description.Trim();
                if (ShowDrawingRef) existing.DrawingRef = NormalizeDrawingRef(Form.DrawingRef);
                existing.Quantity = Form.Quantity.Value;
                existing.UnitWeightKg = Form.UnitWeightKg.Value;
            }
        }

        IsAdding = false;
        Form = new PlanAccessoryForm();
        parent.ErrorMessage = null;
        parent.NotifyChanged();
    }

    public void AskRemove(PlanAccessory item)
    {
        PendingRemoval = item;
        parent.ErrorMessage = null;
        parent.NotifyChanged();
    }

    public void ConfirmRemove()
    {
        if (PendingRemoval is null) return;
        if (Form.EditingId == PendingRemoval.Id) Cancel();
        parent.Items.Remove(PendingRemoval);
        PendingRemoval = null;
        parent.NotifyChanged();
    }

    public void CancelRemove()
    {
        PendingRemoval = null;
        parent.NotifyChanged();
    }

    private static string? NormalizeDrawingRef(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
