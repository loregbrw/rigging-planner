namespace RiggingPlanner.Application.Abstractions;

public interface IFileStore
{
    string AbsolutePath(string relativePath);
    Task<byte[]> ReadAsync(string relativePath);
    Task WriteAsync(string relativePath, byte[] content);
    void Delete(string relativePath);
    void EnsureDirectoryFor(string relativePath);
    string CopyTo(string sourceRelativePath, string targetRelativePath);
}
