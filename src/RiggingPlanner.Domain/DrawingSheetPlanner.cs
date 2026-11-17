namespace RiggingPlanner.Domain;

public static class DrawingSheetPlanner
{
    public const double MarginPt = 10.0;
    public const double ImageGapPt = 12.0;
    public const double PairScaleThreshold = 0.5;

    public const double ViewAreaLeft = 56.5 + MarginPt; // 66.5
    public const double ViewAreaTop = 28.39 + MarginPt; // 38.39
    public const double ViewAreaRight = 566.9 - MarginPt; // 556.9
    public const double ViewAreaBottom = 662.39 - MarginPt; // 652.39

    public const double ViewAreaWidth = ViewAreaRight - ViewAreaLeft; // 490.4
    public const double ViewAreaHeight = ViewAreaBottom - ViewAreaTop; // 614.0

    public static DrawingSheetPlannerResult Plan(IReadOnlyList<DrawingSheetImageInput> images)
    {
        ArgumentNullException.ThrowIfNull(images);
        if (images.Count == 0)
        {
            return new DrawingSheetPlannerResult([]);
        }

        var pages = new List<DrawingSheetPagePlan>();
        var index = 0;
        while (index < images.Count)
        {
            var remaining = images.Count - index;

            if (remaining == 3)
            {
                var (layouts3, scale3) = TryLayout3(images[index], images[index + 1], images[index + 2]);
                if (scale3 > 0 && CheckRule05([images[index], images[index + 1], images[index + 2]], scale3))
                {
                    pages.Add(new DrawingSheetPagePlan(pages.Count + 1, 0, layouts3));
                    index += 3;
                    continue;
                }
            }

            if (remaining >= 2)
            {
                var (layouts2, scale2) = TryLayout2(images[index], images[index + 1]);
                if (scale2 > 0)
                {
                    var scaleAlone1 = ComputeSingleScale(images[index]);
                    var scaleAlone2 = ComputeSingleScale(images[index + 1]);
                    if (scale2 >= PairScaleThreshold * scaleAlone1 && scale2 >= PairScaleThreshold * scaleAlone2)
                    {
                        pages.Add(new DrawingSheetPagePlan(pages.Count + 1, 0, layouts2));
                        index += 2;
                        continue;
                    }
                }
            }

            var layout1 = LayoutSingle(images[index]);
            pages.Add(new DrawingSheetPagePlan(pages.Count + 1, 0, [layout1]));
            index += 1;
        }

        var total = pages.Count;
        var finalized = pages.Select(p => p with { SheetTotal = total }).ToList();
        return new DrawingSheetPlannerResult(finalized);
    }

    public static double ComputeSingleScale(DrawingSheetImageInput img) =>
        Math.Min(ViewAreaWidth / img.WidthPx, ViewAreaHeight / img.HeightPx);

    private static DrawingSheetImageLayout LayoutSingle(DrawingSheetImageInput img)
    {
        var s = ComputeSingleScale(img);
        var w = img.WidthPx * s;
        var h = img.HeightPx * s;
        var x = ViewAreaLeft + (ViewAreaWidth - w) / 2.0;
        var y = ViewAreaTop + (ViewAreaHeight - h) / 2.0;
        var ppi = (img.WidthPx * 72.0) / w;
        return new DrawingSheetImageLayout(img.Id, x, y, w, h, ppi);
    }

    private static (IReadOnlyList<DrawingSheetImageLayout> Layouts, double Scale) TryLayout2(DrawingSheetImageInput i1, DrawingSheetImageInput i2)
    {
        var wCol = (ViewAreaWidth - ImageGapPt) / 2.0;
        var s1Side = Math.Min(wCol / i1.WidthPx, ViewAreaHeight / i1.HeightPx);
        var s2Side = Math.Min(wCol / i2.WidthPx, ViewAreaHeight / i2.HeightPx);
        var scaleSide = Math.Min(s1Side, s2Side);

        var hRow = (ViewAreaHeight - ImageGapPt) / 2.0;
        var s1Stack = Math.Min(ViewAreaWidth / i1.WidthPx, hRow / i1.HeightPx);
        var s2Stack = Math.Min(ViewAreaWidth / i2.WidthPx, hRow / i2.HeightPx);
        var scaleStack = Math.Min(s1Stack, s2Stack);

        if (scaleSide <= 0 && scaleStack <= 0)
        {
            return ([], 0);
        }

        if (scaleStack >= scaleSide)
        {
            var w1 = i1.WidthPx * scaleStack;
            var h1 = i1.HeightPx * scaleStack;
            var x1 = ViewAreaLeft + (ViewAreaWidth - w1) / 2.0;
            var y1 = ViewAreaTop;
            var ppi1 = (i1.WidthPx * 72.0) / w1;

            var w2 = i2.WidthPx * scaleStack;
            var h2 = i2.HeightPx * scaleStack;
            var x2 = ViewAreaLeft + (ViewAreaWidth - w2) / 2.0;
            var y2 = ViewAreaTop + hRow + ImageGapPt;
            var ppi2 = (i2.WidthPx * 72.0) / w2;

            IReadOnlyList<DrawingSheetImageLayout> layouts =
            [
                new(i1.Id, x1, y1, w1, h1, ppi1),
                new(i2.Id, x2, y2, w2, h2, ppi2),
            ];
            return (layouts, scaleStack);
        }
        else
        {
            var w1 = i1.WidthPx * scaleSide;
            var h1 = i1.HeightPx * scaleSide;
            var x1 = ViewAreaLeft;
            var y1 = ViewAreaTop + (ViewAreaHeight - h1) / 2.0;
            var ppi1 = (i1.WidthPx * 72.0) / w1;

            var w2 = i2.WidthPx * scaleSide;
            var h2 = i2.HeightPx * scaleSide;
            var x2 = ViewAreaLeft + wCol + ImageGapPt;
            var y2 = ViewAreaTop + (ViewAreaHeight - h2) / 2.0;
            var ppi2 = (i2.WidthPx * 72.0) / w2;

            IReadOnlyList<DrawingSheetImageLayout> layouts =
            [
                new(i1.Id, x1, y1, w1, h1, ppi1),
                new(i2.Id, x2, y2, w2, h2, ppi2),
            ];
            return (layouts, scaleSide);
        }
    }

    private static (IReadOnlyList<DrawingSheetImageLayout> Layouts, double Scale) TryLayout3(
        DrawingSheetImageInput i1, DrawingSheetImageInput i2, DrawingSheetImageInput i3)
    {
        var hRow3 = (ViewAreaHeight - 2 * ImageGapPt) / 3.0;
        var s1_3s = Math.Min(ViewAreaWidth / i1.WidthPx, hRow3 / i1.HeightPx);
        var s2_3s = Math.Min(ViewAreaWidth / i2.WidthPx, hRow3 / i2.HeightPx);
        var s3_3s = Math.Min(ViewAreaWidth / i3.WidthPx, hRow3 / i3.HeightPx);
        var scale3s = Math.Min(Math.Min(s1_3s, s2_3s), s3_3s);

        var hHalf = (ViewAreaHeight - ImageGapPt) / 2.0;
        var wHalfCol = (ViewAreaWidth - ImageGapPt) / 2.0;
        var s1_1t = Math.Min(ViewAreaWidth / i1.WidthPx, hHalf / i1.HeightPx);
        var s2_1t = Math.Min(wHalfCol / i2.WidthPx, hHalf / i2.HeightPx);
        var s3_1t = Math.Min(wHalfCol / i3.WidthPx, hHalf / i3.HeightPx);
        var scale1t2b = Math.Min(Math.Min(s1_1t, s2_1t), s3_1t);

        var s1_2t = Math.Min(wHalfCol / i1.WidthPx, hHalf / i1.HeightPx);
        var s2_2t = Math.Min(wHalfCol / i2.WidthPx, hHalf / i2.HeightPx);
        var s3_2t = Math.Min(ViewAreaWidth / i3.WidthPx, hHalf / i3.HeightPx);
        var scale2t1b = Math.Min(Math.Min(s1_2t, s2_2t), s3_2t);

        var bestScale = Math.Max(scale3s, Math.Max(scale1t2b, scale2t1b));
        if (bestScale <= 0)
        {
            return ([], 0);
        }

        if (bestScale == scale1t2b)
        {
            var w1 = i1.WidthPx * scale1t2b;
            var h1 = i1.HeightPx * scale1t2b;
            var x1 = ViewAreaLeft + (ViewAreaWidth - w1) / 2.0;
            var y1 = ViewAreaTop;
            var ppi1 = (i1.WidthPx * 72.0) / w1;

            var w2 = i2.WidthPx * scale1t2b;
            var h2 = i2.HeightPx * scale1t2b;
            var x2 = ViewAreaLeft;
            var y2 = ViewAreaTop + hHalf + ImageGapPt;
            var ppi2 = (i2.WidthPx * 72.0) / w2;

            var w3 = i3.WidthPx * scale1t2b;
            var h3 = i3.HeightPx * scale1t2b;
            var x3 = ViewAreaLeft + wHalfCol + ImageGapPt;
            var y3 = ViewAreaTop + hHalf + ImageGapPt;
            var ppi3 = (i3.WidthPx * 72.0) / w3;

            IReadOnlyList<DrawingSheetImageLayout> layouts =
            [
                new(i1.Id, x1, y1, w1, h1, ppi1),
                new(i2.Id, x2, y2, w2, h2, ppi2),
                new(i3.Id, x3, y3, w3, h3, ppi3),
            ];
            return (layouts, scale1t2b);
        }
        else if (bestScale == scale2t1b)
        {
            var w1 = i1.WidthPx * scale2t1b;
            var h1 = i1.HeightPx * scale2t1b;
            var x1 = ViewAreaLeft;
            var y1 = ViewAreaTop;
            var ppi1 = (i1.WidthPx * 72.0) / w1;

            var w2 = i2.WidthPx * scale2t1b;
            var h2 = i2.HeightPx * scale2t1b;
            var x2 = ViewAreaLeft + wHalfCol + ImageGapPt;
            var y2 = ViewAreaTop;
            var ppi2 = (i2.WidthPx * 72.0) / w2;

            var w3 = i3.WidthPx * scale2t1b;
            var h3 = i3.HeightPx * scale2t1b;
            var x3 = ViewAreaLeft + (ViewAreaWidth - w3) / 2.0;
            var y3 = ViewAreaTop + hHalf + ImageGapPt;
            var ppi3 = (i3.WidthPx * 72.0) / w3;

            IReadOnlyList<DrawingSheetImageLayout> layouts =
            [
                new(i1.Id, x1, y1, w1, h1, ppi1),
                new(i2.Id, x2, y2, w2, h2, ppi2),
                new(i3.Id, x3, y3, w3, h3, ppi3),
            ];
            return (layouts, scale2t1b);
        }
        else
        {
            var w1 = i1.WidthPx * scale3s;
            var h1 = i1.HeightPx * scale3s;
            var x1 = ViewAreaLeft + (ViewAreaWidth - w1) / 2.0;
            var y1 = ViewAreaTop;
            var ppi1 = (i1.WidthPx * 72.0) / w1;

            var w2 = i2.WidthPx * scale3s;
            var h2 = i2.HeightPx * scale3s;
            var x2 = ViewAreaLeft + (ViewAreaWidth - w2) / 2.0;
            var y2 = ViewAreaTop + hRow3 + ImageGapPt;
            var ppi2 = (i2.WidthPx * 72.0) / w2;

            var w3 = i3.WidthPx * scale3s;
            var h3 = i3.HeightPx * scale3s;
            var x3 = ViewAreaLeft + (ViewAreaWidth - w3) / 2.0;
            var y3 = ViewAreaTop + 2 * (hRow3 + ImageGapPt);
            var ppi3 = (i3.WidthPx * 72.0) / w3;

            IReadOnlyList<DrawingSheetImageLayout> layouts =
            [
                new(i1.Id, x1, y1, w1, h1, ppi1),
                new(i2.Id, x2, y2, w2, h2, ppi2),
                new(i3.Id, x3, y3, w3, h3, ppi3),
            ];
            return (layouts, scale3s);
        }
    }

    private static bool CheckRule05(IReadOnlyList<DrawingSheetImageInput> images, double groupScale)
    {
        foreach (var img in images)
        {
            var aloneScale = ComputeSingleScale(img);
            if (groupScale < PairScaleThreshold * aloneScale)
            {
                return false;
            }
        }
        return true;
    }
}
