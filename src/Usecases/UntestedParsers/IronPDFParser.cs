using System.Text;
using IronPdf;
using Usecases.UseCaseInterfaces;

namespace UseCases.UntestedParsers
{
    public class IronPDFParser : IParseHtmlToPdf
    {
        public byte[] Execute(string fullHtml, int marginTop = 5, int marginRight = 15, int marginBottom = 5, int marginLeft = 15)
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
            return pdf.BinaryData;
        }
    }
}