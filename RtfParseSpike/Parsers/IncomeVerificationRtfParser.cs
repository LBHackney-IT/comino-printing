using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using RtfParseSpike.Templates;

namespace RtfParseSpike.Parsers
{
    public class IncomeVerificationRtfParser
    {
        public IncomeVerificationTemplate Execute(FileInfo fileInfo)
        {

            var doc = new HtmlDocument();
            doc.Load(fileInfo.ToString());
            var linesOfText = doc.DocumentNode.SelectSingleNode("/html[1]").InnerText.Split("\n");

            return new IncomeVerificationTemplate
            {
                Title = "BENEFIT INCOME VERIFICATION DOCUMENT",
                ClaimNumber = linesOfText.ElementAt(8),
                Name = linesOfText.ElementAt(9)
            };
        }
        private string[] GetLinesInHtmlFile(FileInfo fileInfo)
        {
            var webClient = new WebClient();
            var parsedHtml = webClient.DownloadString(fileInfo.ToString());
            return parsedHtml.Split("\n");
        }
    }

}