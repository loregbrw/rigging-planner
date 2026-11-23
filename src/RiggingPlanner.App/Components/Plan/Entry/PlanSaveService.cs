using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RiggingPlanner.App.Components.Shared;
using RiggingPlanner.Infrastructure.Persistence;

namespace RiggingPlanner.App.Components.Plan.Entry;

public sealed record PlanSaveResult(string? Error, IReadOnlyList<string> ValidationErrors, Guid? PlanId);

public sealed class PlanSaveService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<PlanSaveResult> SaveAsync(PlanDraftState draft, Guid? routePlanId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await SaveAsync(db, draft, routePlanId, ownTransaction: true);
    }

    public async Task<PlanSaveResult> SaveAsync(
        AppDbContext db,
        PlanDraftState draft,
        Guid? routePlanId,
        PlanEntryState? state = null,
        bool ownTransaction = false)
    {
        var validationErrors = Validate(draft.Form);
        if (validationErrors.Count > 0)
        {
            return new PlanSaveResult(null, validationErrors, null);
        }

        state ??= await BuildStateAsync(db, draft);
        var isDraftPlan = !draft.IsPersisted;
        var error = ownTransaction
            ? await PlanEntryPersistence.SaveAsync(db, draft, routePlanId, isDraftPlan, state)
            : await PlanEntryPersistence.SaveCoreAsync(db, draft, routePlanId, isDraftPlan, state);

        return new PlanSaveResult(error, [], error is null ? draft.PlanId : null);
    }

    public static IReadOnlyList<string> Validate(PlanFormModel form)
    {
        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true))
        {
            return [];
        }

        return [.. results
            .Select(x => x.ErrorMessage)
            .Where(x => !string.IsNullOrWhiteSpace(x))!];
    }

    private static async Task<PlanEntryState> BuildStateAsync(AppDbContext db, PlanDraftState draft)
    {
        var state = new PlanEntryState(draft);
        await state.LoadActiveLookupsAsync(db);
        state.RestoreDraftFlags(draft.Plan);
        return state;
    }
}
