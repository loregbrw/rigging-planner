using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using RiggingPlanner.Application.Pdf;

namespace RiggingPlanner.Infrastructure.Pdf;

public sealed class PdfDocumentMerger : IPdfDocumentMerger
{
    public byte[] Merge(IEnumerable<byte[]> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        using var output = new PdfDocument();

        foreach (var document in documents)
        {
            using var input = PdfReader.Open(new MemoryStream(document), PdfDocumentOpenMode.Import);
            for (var index = 0; index < input.PageCount; index++)
            {
                output.AddPage(input.Pages[index]);
            }
        }

        using var stream = new MemoryStream();
        output.Save(stream);
        return stream.ToArray();
    }
}
