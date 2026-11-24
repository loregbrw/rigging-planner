using RiggingPlanner.Application.Abstractions;

namespace RiggingPlanner.Infrastructure.Storage;

public sealed class FileStore(string databasePath) : IFileStore
{
    private readonly string root = Path.GetDirectoryName(databasePath)
        ?? throw new ArgumentException("O caminho do banco de dados deve incluir um diretório.", nameof(databasePath));

    public string AbsolutePath(string relativePath) => Path.GetFullPath(Path.Combine(root, relativePath));

    public async Task<byte[]> ReadAsync(string relativePath) => await File.ReadAllBytesAsync(AbsolutePath(relativePath));

    public async Task WriteAsync(string relativePath, byte[] content)
    {
        EnsureDirectoryFor(relativePath);
        await File.WriteAllBytesAsync(AbsolutePath(relativePath), content);
    }

    public void Delete(string relativePath)
    {
        var absolutePath = AbsolutePath(relativePath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }

    public void EnsureDirectoryFor(string relativePath)
    {
        var directory = Path.GetDirectoryName(AbsolutePath(relativePath));
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }
    }

    public string CopyTo(string sourceRelativePath, string targetRelativePath)
    {
        EnsureDirectoryFor(targetRelativePath);
        File.Copy(AbsolutePath(sourceRelativePath), AbsolutePath(targetRelativePath), overwrite: true);
        return targetRelativePath;
    }
}
