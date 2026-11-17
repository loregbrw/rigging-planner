namespace RiggingPlanner.Application.Pdf;

/// <summary>
/// Joins standalone PDF documents into a single PDF, preserving page order.
/// </summary>
public interface IPdfDocumentMerger
{
    byte[] Merge(IEnumerable<byte[]> documents);
}
