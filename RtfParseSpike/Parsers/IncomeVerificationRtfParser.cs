using System.IO;
using System.Net;
using HtmlAgilityPack;
using RtfParseSpike.Templates;

namespace RtfParseSpike.Parsers
{
    public class IncomeVerificationRtfParser
    {
        public object Execute(FileInfo fileInfo)
        {

            var doc = new HtmlDocument();
            doc.Load(fileInfo.ToString());
            var nodes = doc.DocumentNode.SelectNodes("font");

            return new IncomeVerificationTemplate
            {
                Title = "BENEFIT INCOME VERIFICATION DOCUMENT",
                ClaimNumber = "60065142"
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