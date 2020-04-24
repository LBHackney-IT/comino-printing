using System;
using System.IO;
using System.Threading.Tasks;
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

            var fullHtml = "<!DOCTYPE html><html><head><style>"
                           + css
                           + "</style></head><body>"
                           + htmlInput.Header
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
                    overflow: hidden;
                }}
                .header-table tr td:nth-child(1) {{
                    width: 110mm;
                    min-height: 90mm;
                }}
                .header-table tr td:nth-child(2) {{
                    width: 70mm;
                    min-height: 90mm;
                }}
                .address-table {{
                    width: 110mm;
                    min-height: 90mm;
                    overflow: hidden;
                    table-layout: fixed;
                }}
                .address-table col  :nth-child(1) {{
                    width: 9.6mm;
                }}
                .address-table col  :nth-child(2) {{
                    width: 95.4mm;
                }}
                .address-table col  :nth-child(3) {{
                    width: 5mm;
                }}
                .address-table tr:nth-child(1) td {{
                    height: 25mm;
                }}
                .address-table tr:nth-child(2) td {{
                    height: 4.5mm;
                }}
                .address-table tr:nth-child(3) td {{
                    height: 26.8mm
                }}
                .address-table tr:nth-child(4) td {{
                    min-height: 28.7mm
                }}
                .header-right {{
                    width:70mm;
                    min-height: 90mm;
                }}
                td {{
                    overflow: hidden;
                }}
                {htmlInput.TemplateSpecificCss}
              }}";
        }
    }

    public class NonSupportedException : Exception
    {
    }
}
