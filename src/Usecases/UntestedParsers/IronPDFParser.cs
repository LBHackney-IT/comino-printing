using System.Text;
using Boundary.UseCaseInterfaces;
using IronPdf;
using Usecases.Interfaces;

namespace UseCases.UntestedParsers
{
    public class IronPDFParser : IParseHtmlToPdf
    {
        public void Execute(string fullHtml, string filePath, int marginTop = 5, int marginRight = 15, int marginBottom = 5,
            int marginLeft = 15)
        {
            var renderer = new HtmlToPdf {PrintOptions =
            {
                CssMediaType = PdfPrintOptions.PdfCssMediaType.Print,
                InputEncoding = Encoding.UTF8,
                MarginBottom = marginBottom,
                MarginTop = marginTop,
                MarginLeft = marginLeft,
                MarginRight = marginRight
            }};
            var pdf = renderer.RenderHtmlAsPdf(fullHtml);
            pdf.SaveAs($"./{filePath}.pdf");
        }
    }
}