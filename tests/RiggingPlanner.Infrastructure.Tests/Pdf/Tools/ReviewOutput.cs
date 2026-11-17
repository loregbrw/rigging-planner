namespace RiggingPlanner.Infrastructure.Tests.Pdf.Tools;

/// <summary>
/// Writes review artifacts (overlays, diff metrics) to artifacts/pdf-review/stage-N/
/// at the repository root. The artifacts folder is ignored by git.
/// </summary>
public static class ReviewOutput
{
    public static string RootDirectory()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "RiggingPlanner.slnx")))
        {
            current = current.Parent;
        }

        return current?.FullName
            ?? throw new InvalidOperationException("Repository root could not be located from the test output directory.");
    }

    public static string StageDirectory(int stage) => StageDirectory($"stage-{stage}");

    public static string StageDirectory(string stageName)
    {
        var path = Path.Combine(RootDirectory(), "artifacts", "pdf-review", stageName);
        Directory.CreateDirectory(path);
        return path;
    }

    public static string WriteFile(int stage, string fileName, byte[] content) =>
        WriteFile($"stage-{stage}", fileName, content);

    public static string WriteFile(string stageName, string fileName, byte[] content)
    {
        var path = Path.Combine(StageDirectory(stageName), fileName);
        File.WriteAllBytes(path, content);
        return path;
    }

    public static string WriteText(int stage, string fileName, string content) =>
        WriteText($"stage-{stage}", fileName, content);

    public static string WriteText(string stageName, string fileName, string content)
    {
        var path = Path.Combine(StageDirectory(stageName), fileName);
        File.WriteAllText(path, content);
        return path;
    }
}
