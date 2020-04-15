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
        public LetterTemplate Execute(FileInfo fileInfo)
        {
            var html = GetHTMLFromFilePath(fileInfo);
            var documentNode = GetDocumentNode(html);

            var address = ParseAddress(documentNode);
            var rightSideOfHeader = ParseSenderAddress(documentNode) + ContactDetails(documentNode);

            var mainBody = documentNode.SelectSingleNode("html/body");
            mainBody.RemoveChild(mainBody.SelectSingleNode("table[1]"));

            var templateSpecificCss = documentNode.SelectSingleNode("html/head/style").InnerText;

            return new LetterTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                Header = ParsingHelpers.FormatLetterHeader(address, rightSideOfHeader),
                MainBody = AddLineBreaks(mainBody).OuterHtml,
            };
        }

        private static HtmlNode AddLineBreaks(HtmlNode body)
        {
            var startOfPage2 = body.ChildNodes.ToList()
                .Find(node => node.InnerText == "Revenues &amp; Benefits Service");
            startOfPage2.Attributes.Add("style", "page-break-before: always;");
            return body;
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
            return File.ReadAllText(fileInfo.ToString(), Encoding.UTF8);
        }
    }
}