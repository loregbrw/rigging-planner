using Microsoft.AspNetCore.Components;
using RiggingPlanner.Application.Catalog;
using RiggingPlanner.Application.Plans;
using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.App.Components.Plan.Entry;

public sealed class PlanEntryState
{
    private readonly PlanDraftState draft;

    public PlanEntryState(PlanDraftState draft)
    {
        this.draft = draft;
    }

    public PlanFormModel Form => draft.Form;
    public List<Equipment> EquipmentOptions { get; private set; } = [];
    public List<Rigger> RiggerOptions { get; private set; } = [];
    public List<Client> ClientOptions { get; private set; } = [];
    public bool IsNewClientSelected { get; private set; }
    public bool IsNewRiggerSelected { get; private set; }
    public bool IsNewEquipmentSelected { get; private set; }

    public Client? SelectedClient => ClientOptions.FirstOrDefault(x => x.Id == Form.ClientId);
    public Rigger? SelectedRigger => RiggerOptions.FirstOrDefault(x => x.Id == Form.RiggerId);
    public Equipment? SelectedEquipment => EquipmentOptions.FirstOrDefault(x => x.Id == Form.EquipmentId);
    public bool IsExistingClient => Form.ClientId is not null;
    public bool IsExistingRigger => Form.RiggerId is not null;
    public bool IsExistingEquipment => Form.EquipmentId is not null;

    public bool HasExactClient => ClientOptions.Any(x => string.Equals(x.Name, Form.ClientName?.Trim(),
        StringComparison.CurrentCultureIgnoreCase));
    public bool HasExactRigger => RiggerOptions.Any(x => string.Equals(x.DisplayName, Form.RiggerDisplayName?.Trim(),
        StringComparison.CurrentCultureIgnoreCase));
    public bool HasExactEquipment => EquipmentOptions.Any(x => string.Equals(x.CraneModel, Form.EquipmentModel?.Trim(),
        StringComparison.CurrentCultureIgnoreCase));

    public List<Client> ClientSuggestions => string.IsNullOrWhiteSpace(Form.ClientName)
        ? []
        : ClientOptions.Where(x => x.Name.Contains(Form.ClientName.Trim(),
            StringComparison.CurrentCultureIgnoreCase)).Take(8).ToList();
    public List<Rigger> RiggerSuggestions => string.IsNullOrWhiteSpace(Form.RiggerDisplayName)
        ? []
        : RiggerOptions.Where(x => x.DisplayName.Contains(Form.RiggerDisplayName.Trim(),
            StringComparison.CurrentCultureIgnoreCase)).Take(8).ToList();
    public List<Equipment> EquipmentSuggestions => string.IsNullOrWhiteSpace(Form.EquipmentModel)
        ? []
        : EquipmentOptions.Where(x => x.CraneModel.Contains(Form.EquipmentModel.Trim(),
            StringComparison.CurrentCultureIgnoreCase)).Take(8).ToList();

    public async Task LoadActiveLookupsAsync(ICatalogService catalog)
    {
        EquipmentOptions = [.. await catalog.GetEquipmentsAsync(activeOnly: true)];
        RiggerOptions = [.. await catalog.GetRiggersAsync(activeOnly: true)];
        ClientOptions = [.. await catalog.GetClientsAsync(activeOnly: true)];
    }

    public async Task ConfigureForPlanAsync(ICatalogService catalog, RiggingPlan plan, bool isReadOnly)
    {
        if (isReadOnly)
        {
            await LoadAllLookupsAsync(catalog);
        }
        else
        {
            await AddLinkedOptionsAsync(catalog, plan);
        }

        LoadFromPlan(plan);
    }

    public void RestoreDraftFlags(RiggingPlan? plan)
    {
        UpdateNewFlags(plan);
    }

    public void LoadFromPlan(RiggingPlan plan)
    {
        PlanFormMapper.LoadFromPlan(Form, plan, EquipmentOptions, RiggerOptions);
        UpdateNewFlags(plan);
    }

    public void OnClientNameInput(ChangeEventArgs args)
    {
        Form.ClientName = args.Value?.ToString()?.Trim();
        Form.ClientId = null;
        Form.ClientCnpj = string.Empty;
    }

    public void SelectClient(Client client)
    {
        Form.ClientId = client.Id;
        Form.ClientName = client.Name;
        Form.ClientCnpj = CnpjFormatter.Format(client.Cnpj) ?? string.Empty;
    }

    public void CreateClientFromInput()
    {
        if (string.IsNullOrWhiteSpace(Form.ClientName)) return;
        Form.ClientId = null;
        IsNewClientSelected = true;
    }

    public void RemoveSelectedClient()
    {
        Form.ClientId = null;
        Form.ClientName = string.Empty;
        Form.ClientCnpj = string.Empty;
        IsNewClientSelected = false;
    }

    public void OnClientCnpjInput(ChangeEventArgs args)
    {
        Form.ClientCnpj = CnpjFormatter.Format(args.Value?.ToString()) ?? string.Empty;
    }

    public void OnEquipmentModelInput(ChangeEventArgs args)
    {
        Form.EquipmentModel = args.Value?.ToString()?.Trim() ?? string.Empty;
        Form.EquipmentId = null;
        Form.EquipmentTruckModel = string.Empty;
        Form.EquipmentChassisNumber = string.Empty;
        Form.EquipmentLicensePlate = string.Empty;
        Form.EquipmentManufactureYear = null;
        IsNewEquipmentSelected = false;
        Form.IsNewEquipment = false;
    }

    public void SelectEquipment(Equipment equipment)
    {
        Form.EquipmentId = equipment.Id;
        Form.EquipmentModel = equipment.CraneModel;
        Form.EquipmentTruckModel = equipment.TruckModel;
        Form.EquipmentChassisNumber = equipment.ChassisNumber;
        Form.EquipmentLicensePlate = equipment.LicensePlate;
        Form.EquipmentManufactureYear = equipment.ManufactureYear;
        IsNewEquipmentSelected = false;
        Form.IsNewEquipment = false;
    }

    public void CreateEquipmentFromInput()
    {
        if (string.IsNullOrWhiteSpace(Form.EquipmentModel)) return;
        Form.EquipmentId = null;
        IsNewEquipmentSelected = true;
        Form.IsNewEquipment = true;
    }

    public void RemoveSelectedEquipment()
    {
        Form.EquipmentId = null;
        Form.EquipmentModel = string.Empty;
        Form.EquipmentTruckModel = string.Empty;
        Form.EquipmentChassisNumber = string.Empty;
        Form.EquipmentLicensePlate = string.Empty;
        Form.EquipmentManufactureYear = null;
        IsNewEquipmentSelected = false;
        Form.IsNewEquipment = false;
    }

    public void OnRiggerNameInput(ChangeEventArgs args)
    {
        Form.RiggerDisplayName = args.Value?.ToString()?.Trim() ?? string.Empty;
        Form.RiggerId = null;
        Form.RiggerFullName = string.Empty;
        Form.RiggerRppNumber = string.Empty;
        Form.RiggerCreaNumber = string.Empty;
        IsNewRiggerSelected = false;
        Form.IsNewRigger = false;
    }

    public void SelectRigger(Rigger rigger)
    {
        Form.RiggerId = rigger.Id;
        Form.RiggerDisplayName = rigger.DisplayName;
        Form.RiggerFullName = rigger.FullName;
        Form.RiggerRppNumber = rigger.RppNumber;
        Form.RiggerCreaNumber = rigger.CreaNumber ?? string.Empty;
        IsNewRiggerSelected = false;
        Form.IsNewRigger = false;
    }

    public void CreateRiggerFromInput()
    {
        if (string.IsNullOrWhiteSpace(Form.RiggerDisplayName)) return;
        Form.RiggerId = null;
        IsNewRiggerSelected = true;
        Form.IsNewRigger = true;
    }

    public void RemoveSelectedRigger()
    {
        Form.RiggerId = null;
        Form.RiggerDisplayName = string.Empty;
        Form.RiggerFullName = string.Empty;
        Form.RiggerRppNumber = string.Empty;
        Form.RiggerCreaNumber = string.Empty;
        IsNewRiggerSelected = false;
        Form.IsNewRigger = false;
    }

    private async Task LoadAllLookupsAsync(ICatalogService catalog)
    {
        EquipmentOptions = [.. await catalog.GetEquipmentsAsync()];
        RiggerOptions = [.. await catalog.GetRiggersAsync()];
        ClientOptions = [.. await catalog.GetClientsAsync()];
    }

    private async Task AddLinkedOptionsAsync(ICatalogService catalog, RiggingPlan plan)
    {
        if (plan.ClientId is not null && ClientOptions.All(x => x.Id != plan.ClientId))
        {
            var linkedClient = await catalog.FindClientAsync(plan.ClientId.Value);
            if (linkedClient is not null) ClientOptions.Add(linkedClient);
        }

        if (plan.RiggerId is not null && RiggerOptions.All(x => x.Id != plan.RiggerId))
        {
            var linkedRigger = await catalog.FindRiggerAsync(plan.RiggerId.Value);
            if (linkedRigger is not null) RiggerOptions.Add(linkedRigger);
        }

        if (plan.EquipmentId is not null && EquipmentOptions.All(x => x.Id != plan.EquipmentId))
        {
            var linkedEquipment = await catalog.FindEquipmentAsync(plan.EquipmentId.Value);
            if (linkedEquipment is not null) EquipmentOptions.Add(linkedEquipment);
        }
    }

    private void UpdateNewFlags(RiggingPlan? plan)
    {
        IsNewClientSelected = plan?.ClientId is null && !string.IsNullOrWhiteSpace(Form.ClientName);
        IsNewRiggerSelected = plan?.RiggerId is null && !string.IsNullOrWhiteSpace(Form.RiggerDisplayName);
        IsNewEquipmentSelected = plan?.EquipmentId is null && !string.IsNullOrWhiteSpace(Form.EquipmentModel);
        Form.IsNewRigger = IsNewRiggerSelected;
        Form.IsNewEquipment = IsNewEquipmentSelected;
    }
}
