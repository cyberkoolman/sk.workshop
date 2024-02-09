using Microsoft.KernelMemory.DataFormats.Office;
using Microsoft.KernelMemory.DataFormats.Pdf;

var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Documents");
var dataFiles = Directory.EnumerateFiles(dataDirectory, "*.*", SearchOption.AllDirectories);

var pptxFiles = dataFiles.Where(file => file.EndsWith(".pptx"));
foreach (var pptxFile in pptxFiles)
{
    var text = new MsPowerPointDecoder().DocToText(pptxFile,
        withSlideNumber: true,
        withEndOfSlideMarker: false,
        skipHiddenSlides: true);
    Console.WriteLine(text);
}

Console.ReadLine();
var pdfFiles = dataFiles.Where(file => file.EndsWith(".pdf"));
foreach (var pdfFile in pdfFiles)
{
    var pages = new PdfDecoder().DocToText(pdfFile);
    foreach (var page in pages)
    {
        Console.WriteLine(page.Text);
    }
}
