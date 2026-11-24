# PDF export

## Assembly

`IExportService.BuildAsync(planId)` loads the saved plan, builds each part and delegates to `FullPlanPdfAssembler`, which joins the documents in a fixed order:

1. summary sheet (`ISummarySheetRenderer`)
2. drawing sheets with images (`IDrawingSheetsRenderer`)
3. plan ART attachment (when present)
4. operational procedure (`IOperationalProcedureRenderer`)
5. equipment safety report (when present)

Missing optional parts are skipped. Warnings are returned with the document, e.g. missing ART or no valid equipment report. The App writes the bytes to `Downloads/plano-completo-{PlanNumber}.pdf` and opens the viewer.

Renderers use QuestPDF with absolute A4 coordinates measured from an approved layout; `PdfDocumentMerger` uses PDFsharp and `DrawingSheetTextMeasurer` uses SkiaSharp for ink bounds. `PdfFontRegistry` registers the embedded fonts once for the process. Fonts and their licences are in `Infrastructure/Pdf/Fonts`.

## Snapshots

The procedure is rendered from `PlanProcedureSnapshot` for finalized plans and from live data otherwise. Exporting must not create or modify a snapshot.

## Reference fixtures

The original approved PDF and the word-level JSON extracts contain third-party client data and are **not versioned**. They are ignored by `.gitignore` under `tests/RiggingPlanner.Infrastructure.Tests/Fixtures`. If you have an authorized local copy, place it there as `reference-plan.pdf` / `summary-sheet.reference.json` / `drawing-sheet.reference.json` to enable the letter-level comparison tests. Without those files those tests return early; structural PDF tests still run.
