# Design system

## CSS layers

`wwwroot/css/00-layers.css` fixes the cascade order:

```
reset → tokens → base → layout → components → utilities
```

Files are numbered `00`–`05` and loaded in that order by `wwwroot/index.html`. Put new rules in the matching layer instead of adding one-off inline styles.

## Tokens

`01-tokens.css` defines the palette (`--rp-primary-*`, `--rp-neutral-*`, status colours), spacing (`--rp-space-*`), typography (`--rp-text-*`, Fredoka variable font), radii, shadows and z-index values. Use tokens for new UI; raw colours should only appear when a token does not fit.

## Shared components

Located in `src/RiggingPlanner.App/Components/Shared`:

- `RpCard`, `RpPageHeader` — page structure.
- `RpStepper`, `RpPlanNavigation`, `RpPlanStepLayout` — navigation and actions.
- `RpDecimalInput` — required for decimal fields; parses `pt-BR` input and shows validation messages.
- `RpTagPicker` — autocomplete for catalog selection.
- `RpFileAttachmentBox` — file display and upload.
- `RpPlanLeaveGuard` — unsaved-changes guard.

`Components/Plan` holds step-specific composition (`Entry`, `Capacity`, `Attachments`, `Preview`, `Export`, `Workflow`).

## Conventions

- Class names use the `rp-` prefix with BEM-like suffixes (`rp-card`, `rp-btn--primary`).
- Layout is a fixed sidebar plus content area with one responsive breakpoint.
- User-facing strings are Portuguese; code identifiers are English.
- Company logos are under `wwwroot/images` and `Resources/AppIcon`; they are part of the product and appear in the exported procedure PDF.
