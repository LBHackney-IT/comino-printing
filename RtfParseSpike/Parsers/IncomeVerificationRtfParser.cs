using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using RtfParseSpike.Templates;
using RtfPipe;

namespace RtfParseSpike.Parsers
{
    public class IncomeVerificationRtfParser
    {
        public IncomeVerificationTemplate Execute(FileInfo fileInfo)
        {
            var html = GetHTMLFromFilePath(fileInfo);

            var documentNode = GetDocumentNode(html);

            return new IncomeVerificationTemplate
            {
                Title = ParseSingleNode(documentNode, "/div/p[1]/strong"),
                ClaimNumber = ParseSingleNode(documentNode, "/div/p[3]"),
                Name = ParseSingleNode(documentNode, "/div/p[4]"),
                Address = ParseAddress(documentNode),
                Postcode = ParseSingleNode(documentNode, "/div/p[8]"),
                PeriodStartDateTable = ParseTable(documentNode, "/div/table[1]/tr"),
                ReasonForAssessmentTable = ParseTable(documentNode,"/div/table[2]/tr" ),
                IncomeDetailsTable = ParseTable(documentNode, "/div/table[3]/tr"),
                OverpaymentTableHeaders = ParseSingleNode(documentNode, "/div/p[13]"),
                OverpaymentTable = ParseTable(documentNode, "/div/table[4]/tr"),
                AdditionalCommentTable = ParseTable(documentNode, "/div/table[5]/tr")
            };
        }

        private static List<string> ParseAddress(HtmlNode documentNode)
        {
            return documentNode.SelectNodes("/div/p").ToList().GetRange(4, 3).Select(node => node.InnerText).ToList();
        }

        private static string ParseSingleNode(HtmlNode documentNode, string xpath)
        {
            return documentNode.SelectSingleNode(xpath).InnerText;
        }

        private static List<List<string>> ParseTable(HtmlNode documentNode, string xpath)
        {
            return documentNode.SelectNodes(xpath)
                .Select(row => row.ChildNodes.Select(AddNewLinesToTextForParagraphTags).ToList()).ToList();
        }

        private static string AddNewLinesToTextForParagraphTags(HtmlNode html)
        {
            return html.ChildNodes.Aggregate("", (agg, node) =>
            {
                if (node.Name == "p")
                {
                    return agg + "\n" + node.InnerText + "\n";
                }

                return agg + node.InnerText;
            }).TrimEnd('\n');
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

            var letter = File.ReadAllText(fileInfo.ToString(), Encoding.UTF8);
            return Rtf.ToHtml(letter);
        }
    }

}