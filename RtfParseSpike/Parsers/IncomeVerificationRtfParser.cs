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

            var title = documentNode.SelectSingleNode("/div/p[1]/strong").InnerText;
            var claimNumber = documentNode.SelectSingleNode("/div/p[3]").InnerText;
            var addresseeName = documentNode.SelectSingleNode("/div/p[4]").InnerText;
            var address = documentNode.SelectNodes("/div/p").ToList().GetRange(4, 3).Select(node => node.InnerText);
            var postcode = documentNode.SelectSingleNode("/div/p[8]").InnerText;
            var periodStartDate = documentNode.SelectNodes("/div/table[1]/tr/td").Select(node => node.InnerText);
            var reasonForAssessment = documentNode.SelectSingleNode("/div/table[2]/tr/td").InnerHtml.Replace("</p>", "\n").Replace("<p style=\"margin:0;\">", "\n").TrimEnd('\n');

            return new IncomeVerificationTemplate
            {
                Title = title,
                ClaimNumber = claimNumber,
                Name = addresseeName,
                Address = address.ToList(),
                Postcode = postcode,
                PeriodStartDateTable = new List<List<string>> {periodStartDate.ToList()},
                ReasonForAssessmentTable = new List<List<string>> {new List<string>{reasonForAssessment}}
            };
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