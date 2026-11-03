using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.App.Components.Shared;

public sealed class PlanDraftState
{
    public PlanFormModel Form { get; } = new();
    public RiggingPlan? Plan { get; set; }
    public List<PlanAccessory> Accessories { get; } = [];
    public Guid? PlanId { get; set; }
    public bool IsInitialized { get; set; }
    public bool IsPersisted { get; set; }

    public void Reset()
    {
        Plan = null;
        PlanId = null;
        Accessories.Clear();
        IsInitialized = false;
        IsPersisted = false;
        Form.OperationName = string.Empty;
        Form.Location = string.Empty;
        Form.PlanDate = DateTime.Today;
        Form.ClientId = null;
        Form.ClientName = null;
        Form.ClientCnpj = string.Empty;
        Form.LoadDescription = string.Empty;
        Form.WeightKg = null;
        Form.LengthM = null;
        Form.WidthM = null;
        Form.HeightM = null;
        Form.Observations = null;
        Form.EquipmentId = null;
        Form.EquipmentModel = string.Empty;
        Form.EquipmentTruckModel = string.Empty;
        Form.EquipmentChassisNumber = string.Empty;
        Form.EquipmentLicensePlate = string.Empty;
        Form.EquipmentManufactureYear = null;
        Form.IsNewEquipment = false;
        Form.RiggerId = null;
        Form.RiggerFullName = string.Empty;
        Form.RiggerDisplayName = string.Empty;
        Form.RiggerRppNumber = string.Empty;
        Form.RiggerCreaNumber = string.Empty;
    }
}
