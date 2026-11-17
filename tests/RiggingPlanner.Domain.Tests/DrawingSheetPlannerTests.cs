using Xunit;

namespace RiggingPlanner.Domain.Tests;

public class DrawingSheetPlannerTests
{
    [Fact]
    public void Plan_EmptyList_ReturnsNoPages()
    {
        var result = DrawingSheetPlanner.Plan([]);
        Assert.Empty(result.Pages);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    public void Plan_VariousImageCounts_SatisfiesAllInvariants(int count)
    {
        var images = Enumerable.Range(1, count)
            .Select(i => new DrawingSheetImageInput(
                Id: Guid.NewGuid(),
                WidthPx: 800 + (i % 3) * 200,
                HeightPx: 600 + (i % 2) * 150,
                Name: $"Image {i}"))
            .ToList();

        var result = DrawingSheetPlanner.Plan(images);

        Assert.NotEmpty(result.Pages);
        var totalSheets = result.Pages.Count;

        var allPlacedImageIds = new List<Guid>();

        for (var i = 0; i < result.Pages.Count; i++)
        {
            var page = result.Pages[i];
            Assert.Equal(i + 1, page.SheetNumber);
            Assert.Equal(totalSheets, page.SheetTotal);
            Assert.InRange(page.Images.Count, 1, 3);

            foreach (var imgLayout in page.Images)
            {
                allPlacedImageIds.Add(imgLayout.ImageId);

                // Within view area bounds
                Assert.True(imgLayout.X >= DrawingSheetPlanner.ViewAreaLeft - 0.01);
                Assert.True(imgLayout.Y >= DrawingSheetPlanner.ViewAreaTop - 0.01);
                Assert.True(imgLayout.X + imgLayout.Width <= DrawingSheetPlanner.ViewAreaRight + 0.01);
                Assert.True(imgLayout.Y + imgLayout.Height <= DrawingSheetPlanner.ViewAreaBottom + 0.01);
                Assert.True(imgLayout.Width > 0);
                Assert.True(imgLayout.Height > 0);
                Assert.True(imgLayout.EffectivePpi > 0);
            }

            // Check no overlap and min gap between images on the same sheet
            for (var a = 0; a < page.Images.Count; a++)
            {
                for (var b = a + 1; b < page.Images.Count; b++)
                {
                    var rect1 = page.Images[a];
                    var rect2 = page.Images[b];

                    // Check separation: either rect1 is left of rect2 (+ gap) or above (+ gap)
                    var separatedHorizontally = (rect1.X + rect1.Width + DrawingSheetPlanner.ImageGapPt - 0.01 <= rect2.X) ||
                                                (rect2.X + rect2.Width + DrawingSheetPlanner.ImageGapPt - 0.01 <= rect1.X);
                    var separatedVertically = (rect1.Y + rect1.Height + DrawingSheetPlanner.ImageGapPt - 0.01 <= rect2.Y) ||
                                              (rect2.Y + rect2.Height + DrawingSheetPlanner.ImageGapPt - 0.01 <= rect1.Y);

                    Assert.True(separatedHorizontally || separatedVertically, $"Images on sheet {page.SheetNumber} overlap or do not maintain 12pt gap.");
                }
            }
        }

        // All images appear exactly once in order
        Assert.Equal(images.Select(x => x.Id), allPlacedImageIds);
    }
}
