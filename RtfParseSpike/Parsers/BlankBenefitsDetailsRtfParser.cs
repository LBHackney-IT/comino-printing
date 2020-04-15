using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using RtfParseSpike.Templates;

namespace RtfParseSpike.Parsers
{
    public class BlankBenefitsRtfParser
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
                MainBody = mainBody.OuterHtml,
            };
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
                .ToList().GetRange(8, 8);
            rows.ElementAt(0).RemoveChild(rows.ElementAt(0).SelectSingleNode("td[1]"));
            rows.ElementAt(1).RemoveChild(rows.ElementAt(1).SelectSingleNode("td[1]"));
            return $"<table> {rows.Aggregate("", (accRows, row) => accRows + row.OuterHtml)} </table>";
        }

        private static string ParseAddress(HtmlNode documentNode)
        {
            var name = documentNode.SelectSingleNode("/html/body/table[1]/tr[9]/td[1]");
            name.Name = "p";
            return name.OuterHtml + documentNode.SelectSingleNode("/html/body/table[1]/tr[10]/td[1]").ChildNodes
                       .Aggregate("", (agg, node) => agg + node.OuterHtml);
        }

        private static string GetHTMLFromFilePath(FileInfo fileInfo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return  File.ReadAllText(fileInfo.ToString(), Encoding.UTF8);
        }

        private static HtmlNode GetDocumentNode(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }
    }
}