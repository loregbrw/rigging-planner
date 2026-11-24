using RiggingPlanner.Application.Catalog;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.Application.Plans;

public static class PlanFormMapper
{
    public static void ApplyToDraftPlan(PlanFormModel form, RiggingPlan plan)
    {
        plan.OperationName = form.OperationName.Trim();
        plan.Location = form.Location.Trim();
        plan.PlanDate = form.PlanDate;
        plan.ClientId = form.ClientId;
        plan.ClientName = form.ClientName?.Trim();
        plan.ClientCnpj = form.ClientCnpj;
        plan.LoadDescription = form.LoadDescription.Trim();
        plan.LengthM = form.LengthM;
        plan.WidthM = form.WidthM;
        plan.HeightM = form.HeightM;
        plan.WeightKg = form.WeightKg;
        plan.EquipmentId = form.EquipmentId;
        plan.CraneModel = form.EquipmentModel;
        plan.TruckModel = form.EquipmentTruckModel;
        plan.ChassisNumber = form.EquipmentChassisNumber;
        plan.LicensePlate = form.EquipmentLicensePlate;
        plan.ManufactureYear = form.EquipmentManufactureYear;
        plan.RiggerId = form.RiggerId;
        plan.RiggerName = form.RiggerDisplayName.Trim();
        plan.RiggerRpp = form.RiggerRppNumber.Trim();
        plan.RiggerCrea = string.IsNullOrWhiteSpace(form.RiggerCreaNumber) ? null : form.RiggerCreaNumber.Trim();
        plan.Observations = string.IsNullOrWhiteSpace(form.Observations) ? null : form.Observations.Trim();
    }

    public static void ApplyToSavedPlan(PlanFormModel form, RiggingPlan plan, Client? client, Equipment? equipment,
        Rigger? rigger, bool formatCnpj = true)
    {
        plan.PlanDate = form.PlanDate;
        plan.Location = form.Location.Trim();
        plan.OperationName = form.OperationName.Trim();
        plan.ClientId = client?.Id;
        plan.ClientName = client?.Name ?? form.ClientName?.Trim();
        plan.ClientCnpj = client?.Cnpj ?? (formatCnpj ? CnpjFormatter.Format(form.ClientCnpj) : form.ClientCnpj?.Trim());
        plan.LoadDescription = form.LoadDescription.Trim();
        plan.LengthM = form.LengthM;
        plan.WidthM = form.WidthM;
        plan.HeightM = form.HeightM;
        plan.WeightKg = form.WeightKg;
        plan.Observations = string.IsNullOrWhiteSpace(form.Observations) ? null : form.Observations.Trim();
        plan.EquipmentId = equipment?.Id;
        plan.RiggerId = rigger?.Id;
    }

    public static void LoadFromPlan(PlanFormModel form, RiggingPlan plan,
        IReadOnlyCollection<Equipment> equipmentOptions, IReadOnlyCollection<Rigger> riggerOptions)
    {
        form.OperationName = plan.OperationName;
        form.Location = plan.Location;
        form.PlanDate = plan.PlanDate;
        form.ClientId = plan.ClientId;
        form.ClientName = plan.ClientName;
        form.ClientCnpj = CnpjFormatter.Format(plan.ClientCnpj) ?? string.Empty;
        form.LoadDescription = plan.LoadDescription;
        form.WeightKg = plan.WeightKg;
        form.LengthM = plan.LengthM;
        form.WidthM = plan.WidthM;
        form.HeightM = plan.HeightM;
        form.Observations = plan.Observations;
        form.EquipmentId = plan.EquipmentId;
        var linkedEquipment = equipmentOptions.FirstOrDefault(x => x.Id == plan.EquipmentId);
        form.EquipmentModel = linkedEquipment?.CraneModel ?? plan.CraneModel ?? string.Empty;
        form.EquipmentTruckModel = linkedEquipment?.TruckModel ?? plan.TruckModel ?? string.Empty;
        form.EquipmentChassisNumber = linkedEquipment?.ChassisNumber ?? plan.ChassisNumber ?? string.Empty;
        form.EquipmentLicensePlate = linkedEquipment?.LicensePlate ?? plan.LicensePlate ?? string.Empty;
        form.EquipmentManufactureYear = linkedEquipment?.ManufactureYear ?? plan.ManufactureYear;
        form.RiggerId = plan.RiggerId;
        var linkedRigger = riggerOptions.FirstOrDefault(x => x.Id == plan.RiggerId);
        form.RiggerDisplayName = linkedRigger?.DisplayName ?? plan.RiggerName ?? string.Empty;
        form.RiggerFullName = linkedRigger?.FullName ?? plan.RiggerName ?? string.Empty;
        form.RiggerRppNumber = linkedRigger?.RppNumber ?? plan.RiggerRpp ?? string.Empty;
        form.RiggerCreaNumber = linkedRigger?.CreaNumber ?? plan.RiggerCrea ?? string.Empty;
    }
}
