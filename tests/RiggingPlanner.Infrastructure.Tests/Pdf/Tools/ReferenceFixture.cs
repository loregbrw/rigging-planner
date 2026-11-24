namespace RiggingPlanner.Infrastructure.Tests.Pdf.Tools;

public static class ReferenceFixture
{
    public static string Path(string fileName) =>
        System.IO.Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);

    public static bool Exists(string fileName) => File.Exists(Path(fileName));
}
