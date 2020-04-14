using System.Text;
using IronPdf;
using RtfParseSpike.Templates;

namespace RtfParseSpike
{
    public class CreatePDF
    {
        public byte[] Execute(ChangesInCircsICLTemplate htmlInput)
        {
            var css = CompileCss(htmlInput);

            var fullHtml = "<!DOCTYPE html><html><head><style>" +
                           css + "</style></head><body>" + htmlInput.Header
                           + htmlInput.MainBody + "</body></html>";

            var renderer = new HtmlToPdf {PrintOptions =
            {
                CssMediaType = PdfPrintOptions.PdfCssMediaType.Print,
                InputEncoding = Encoding.UTF8,
                MarginBottom = 5,
                MarginTop = 5,
                MarginLeft = 15,
                MarginRight = 15
            }};
            var pdf = renderer.RenderHtmlAsPdf(fullHtml);
            return pdf.BinaryData;
        }

        private static string CompileCss(ChangesInCircsICLTemplate htmlInput)
        {
            return $@"@media print {{
                body {{ font-family: Helvetica, sans-serif; }}
                .header-table {{
                    width: 180mm;
                    height: 90mm;
                    table-layout:fixed;
                    overflow: hidden;
                    tr td {{
                        :nth-child(1) {{
                            width: 110mm;
                            height: 90mm;
                        }}
                        :nth-child(2) {{
                            width: 70mm;
                            height: 90mm;
                        }}
                        overflow: hidden;
                    }}
                }}
                .address-table {{
                    width: 110mm;
                    height: 90mm;
                    overflow: hidden;
                    table-layout: fixed;
                    col {{
                        :nth-child(1) {{
                            width: 9.6mm;
                        }},
                        :nth-child(2) {{
                            width: 95.4mm;
                        }},
                        :nth-child(3) {{
                            width: 5mm;
                        }}
                    }}
                    tr {{
                        :nth-child(1) {{
                            td {{
                                height: 25mm;
                            }}
                        }},
                        :nth-child(2) {{
                            td {{
                                height: 4.5mm;
                            }}
                        }},
                        :nth-child(3) {{
                            td {{
                                height: 26.8mm
                            }}
                        }},
                        :nth-child(4) {{
                            td {{
                                height: 28.7mm
                            }}
                        }}

                        td {{
                            overflow: hidden;
                        }}
                    }}
                }}
                .header-right {{
                    width:70mm;
                    height: 90mm;
                    x-overflow: hidden;
                }}
                {htmlInput.TemplateSpecificCss}
              }}";
        }
    }
}