using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using RtfParseSpike.Templates;

namespace RtfParseSpike.Parsers
{
    public class ChangesInCircsICL
    {
        public ChangesInCircsICLTemplate Execute(FileInfo fileInfo)
        {
            var html = GetHTMLFromFilePath(fileInfo);
            var documentNode = GetDocumentNode(html);
            var address = ParseAddress(documentNode);
            var rightSideOfHeader = ParseSenderAddress(documentNode) + ContactDetails(documentNode);
            // add overflow hidden to everything
            var headerTable = FormatHeaderTable(address, rightSideOfHeader);

            var mainBody = documentNode.SelectSingleNode("html/body");
            mainBody.RemoveChild(mainBody.SelectSingleNode("table[1]"));
            var templateSpecificCss = documentNode.SelectSingleNode("html/head/style").InnerText;

            return new ChangesInCircsICLTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                Header = headerTable,
                MainBody = mainBody.OuterHtml,
            };
        }

        private static string FormatHeaderTable(string address, string rightSideHeader)
        {
            var addressTable = "<table class=\"address-table\" >" +
                              "<col width=\"9.6mm\" />" +
                              "<col width=\"95.4mm\" />" +
                              "<col width=\"5mm\" />" +
                              "<tr> <td>&nbsp;</td> <td>&nbsp;</td> <td>&nbsp;</td> </tr>" +
                              "<tr> <td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                              "<tr><td>&nbsp;</td>" +
                              "<td>" +
                              address +
                              "</td>" +
                              "<td></td></tr>" +
                              "<tr ><td></td><td></td><td></td></tr>" +
                              "</table>";
            var rightHandHeaderSpace = "<div class=\"header-right\">"+ rightSideHeader +"</div>";
            return "<table class=\"header-table\">" +
                   "<tr>" +
                   "<td>" + addressTable + "</td>" +
                   "<td>" + rightHandHeaderSpace + "</td>" +
                   "</tr>" +
                   "</table>";
        }

        private static string ParseSenderAddress(HtmlNode documentNode)
        {
            var table = documentNode.SelectNodes("html/body/table[1]/tr").ToList();
            return table
                .GetRange(1, 7)
                .Aggregate("", (accString, node) => 
                    accString + $"<p> {node.SelectNodes("td").Last().InnerHtml} </p>");
        }

        private static string ContactDetails(HtmlNode documentNode)
        {
            var rows = documentNode.SelectNodes("/html/body/table[1]/tr")
                .ToList()
                .GetRange(9, 8)
                .Aggregate("", (accRows, row) => accRows + row.OuterHtml);
            return $"<table> {rows} </table>";
        }

        private static string ParseAddress(HtmlNode documentNode)
        {
            var name = documentNode.SelectSingleNode("/html/body/table[1]/tr[8]/td[1]");
            name.Name = "p";
            return name.OuterHtml + documentNode.SelectSingleNode("/html/body/table[1]/tr[9]/td[1]").ChildNodes
                .Aggregate("", (agg, node) => agg + node.OuterHtml);
        }

        private static HtmlNode GetDocumentNode(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        private static string GetHTMLFromFilePath(FileInfo fileInfo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return File.ReadAllText(fileInfo.ToString(), Encoding.UTF8);
        }
    }
}