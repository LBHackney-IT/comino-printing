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

            return new IncomeVerificationTemplate
            {
                Title = title,
                ClaimNumber = claimNumber,
                Name = addresseeName
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