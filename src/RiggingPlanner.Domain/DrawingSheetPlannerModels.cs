namespace RiggingPlanner.Domain;

public sealed record DrawingSheetImageInput(
    Guid Id,
    int WidthPx,
    int HeightPx,
    string Name = "");

public sealed record DrawingSheetImageLayout(
    Guid ImageId,
    double X,
    double Y,
    double Width,
    double Height,
    double EffectivePpi);

public sealed record DrawingSheetPagePlan(
    int SheetNumber,
    int SheetTotal,
    IReadOnlyList<DrawingSheetImageLayout> Images);

public sealed record DrawingSheetPlannerResult(
    IReadOnlyList<DrawingSheetPagePlan> Pages);
