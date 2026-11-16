using RiggingPlanner.Domain.Entities;

namespace RiggingPlanner.App.Components.Plan.Attachments;

public sealed class AttachmentDraftState
{
    private const string StagingRoot = "rigging-plan-staging";

    public Guid SessionId { get; private set; } = Guid.NewGuid();

    public string StagingDirectory => Path.Combine(FileSystem.AppDataDirectory, StagingRoot, SessionId.ToString("N"));

    public string EnsureStagingDirectory()
    {
        Directory.CreateDirectory(StagingDirectory);
        return StagingDirectory;
    }

    public string ToRelativeStagingPath(string fileName)
        => Path.Combine(StagingRoot, SessionId.ToString("N"), fileName);

    public void Reset()
    {
        DeleteStagingDirectory();
        SessionId = Guid.NewGuid();
    }

    public void DeleteStagingDirectory()
    {
        try
        {
            if (Directory.Exists(StagingDirectory))
            {
                Directory.Delete(StagingDirectory, recursive: true);
            }
        }
        catch
        {
        }
    }

    public static bool IsStaged(StoredFile? file)
        => file is not null && file.RelativePath.StartsWith(StagingRoot, StringComparison.OrdinalIgnoreCase);

    public static void TryDeleteStagedFile(StoredFile? file)
    {
        if (!IsStaged(file))
        {
            return;
        }

        try
        {
            var absolutePath = Path.Combine(FileSystem.AppDataDirectory, file!.RelativePath);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }
        catch
        {
        }
    }

    public static void CleanupStaleSessions()
    {
        try
        {
            var root = Path.Combine(FileSystem.AppDataDirectory, StagingRoot);
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
        catch
        {
        }
    }
}
