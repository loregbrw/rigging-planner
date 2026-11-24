# Domain rules

These rules are deliberate product decisions. Do not "simplify" them away.

## Plan number

`PlanNumber` produces `AAMMNNN`: two-digit year, two-digit month, three-digit sequence. The month comes from `PlanDate`, never from the wall clock. The sequence is stored in `PlanNumberCounter` keyed by `YYMM` and incremented inside the save transaction; the `RiggingPlans.PlanNumber` column is unique.

## Snapshots

`RiggingPlan` copies client, equipment and rigger identification fields into the plan itself. Changing a master record later must never change an issued plan. Keep the denormalized columns; do not replace them with navigation-only reads.

`PlanProcedureSnapshot` freezes the operational procedure content, crane model code and SHA-256 when a plan is finalized. Drafts and exports use live data; finalized plans use the snapshot. Exporting never creates or modifies a snapshot.

Accessories (`PlanAccessory`) and drawings are per-plan rows with a stable `SortOrder`.

## Derived values

Weights, totals and crane utilization are computed for display by `PlanTotals` and the document builders. They are never persisted as columns. The same applies to the formatted values in `PlanValueFormatter` and the printed `SummarySheetModel` / stamp models.

## Status and lifecycle

- `draft`: editable.
- `finalized`: read-only; the procedure snapshot exists.

Registry records in use are inactivated instead of being hard-deleted. A plan can be soft-deleted (`DeletedAt`) and later removed definitively. Finalizing twice is rejected.

## Operational procedure

The procedure text is fixed in `OperationalProcedureService` and embedded in the app. It is the same for every crane model; variables (operation name, ART, plan number, crane model code) are substituted at render time. Updating the text requires a new app version and a matching edit to `docs/procedure-spec.md`; previously finalized plans keep their own snapshot.

## UI rules

- All user-facing text is Portuguese; identifiers are English.
- Use `RpDecimalInput` for decimal fields so `pt-BR` parsing is consistent.
- There is no autosave between steps; only an explicit save writes the draft.
- `RpPlanLeaveGuard` protects unsaved changes and the page title is `Plano {PlanNumber} — {OperationName}`.
- Do not add external component libraries; shared visuals live in `Components/Shared` and `wwwroot/css`.
