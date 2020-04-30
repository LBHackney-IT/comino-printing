using System;
using System.IO;
using System.Threading.Tasks;
using Usecases;
using Usecases.Domain;
using Usecases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UseCases
{
    public class ConvertHtmlToPdf : IConvertHtmlToPdf
    {
        private readonly IParseHtmlToPdf _parseHtmlToPdf;
        private readonly IGetParser _getParser;

        public ConvertHtmlToPdf(IParseHtmlToPdf parseHtmlToPdf, IGetParser getParser)
        {
            _parseHtmlToPdf = parseHtmlToPdf;
            _getParser = getParser;
        }

        public async Task Execute(string htmlDocument, string documentType, string documentId)
        {
            ILetterParser parser = _getParser.ForType(documentType);
            if (parser == null)
            {
                Console.WriteLine($"Letter type invalid {documentType}");
                throw new NonSupportedException();
            }
            var htmlInput = parser.Execute(htmlDocument);

            var css = CompileCss(htmlInput);

            var header = ParsingHelpers.FormatLetterHeader(htmlInput.AddressLines, htmlInput.RightSideOfHeader);

            var fullHtml = "<!DOCTYPE html><html><head><style>"
                           + css
                           + "</style></head><body>"
                           + header
                           + htmlInput.MainBody
                           + "</body></html>";

            var pdfBytes = await _parseHtmlToPdf.Convert(fullHtml, documentId);

            Console.WriteLine("Writing PDF to temp file");
            await File.WriteAllBytesAsync($"/tmp/{documentId}.pdf", pdfBytes);
            Console.WriteLine("Successfully written to file");
        }

        private static string CompileCss(LetterTemplate htmlInput)
        {
            return $@"@media print {{
                body {{
                    font-family: Helvetica, sans-serif;
                }}
                .header-table {{
                    width: 180mm;
                    min-height: 90mm;
                    table-layout:fixed;
                }}
                .header-table tr td:nth-child(1) {{
                    width: 110mm;
                    min-height: 90mm;
                }}
                .header-table tr td:nth-child(2) {{
                    width: 70mm;
                    min-height: 90mm;
                }}
                .header-right {{
                    width:70mm;
                    min-height: 90mm;
                }}
                #address {{
                    x-overflow: hidden;
                    width: 110mm;
                    position: absolute;
                    top: 34.5mm;
                    left: 9.6mm;
                    padding: 2mm;
                    font-size: 8pt;
                }}
                {htmlInput.TemplateSpecificCss}
              }}";
        }
    }

    public class NonSupportedException : Exception
    {
    }
}
