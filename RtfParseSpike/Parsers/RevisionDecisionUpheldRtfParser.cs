using System;
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

    public class RevisionDecisionUpheldRtfParser
    {
        public RevisionDecisionUpheldTemplate Execute(FileInfo fileInfo)
        {
            var html = GetHTMLFromFilePath(fileInfo);
            var documentNode = GetDocumentNode(html);
            var headerTable = documentNode.SelectSingleNode("/div/table");

            return new RevisionDecisionUpheldTemplate
            {
                HeaderTableRows = ParseHeaderTable(headerTable),
                Content = ParseContent(documentNode)
            };
        }

        private static string GetHTMLFromFilePath(FileInfo fileInfo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var letter = File.ReadAllText(fileInfo.ToString(), Encoding.UTF8);
            return Rtf.ToHtml(letter);
        }

        private static HtmlNode GetDocumentNode(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        private static string ParseSingleNode(HtmlNode documentNode, string xpath)
        {
            return documentNode.SelectSingleNode(xpath).InnerText;
        }

        private static List<string> ParseHeaderTable(HtmlNode headerTable)
        {
            var cells = headerTable.SelectNodes("//td");
            foreach (var cell in cells)
            {
                cell.Attributes.Remove("style");
            }

            var links = headerTable.SelectNodes("//a");
            foreach (var link in links)
            {
                var linkParent = link.ParentNode;
                var linkText = link.InnerText;

                var newNode = HtmlNode.CreateNode("<span>" + linkText + "</span>");
                newNode.Attributes.Add("style", "text-decoration:underline;");
                
                linkParent.ReplaceChild(newNode, link);
            }

            var spans = headerTable.SelectNodes("//span");
            foreach (var span in spans)
            {
                if (span.InnerText.Trim().Length == 0)
                {
                    span.Remove();
                }
            }

            var tableRows = headerTable.SelectNodes("//tr");

            return tableRows.Select(row => row.OuterHtml).ToList();
        }

        private static List<string> ParseContent(HtmlNode parentNode)
        {
            var images = parentNode.SelectNodes("//img");
            if (images != null)
            {
                foreach (var image in images)
                {
                    // TODO: handle images correctly
                    image.Attributes.Remove("src");
                }
            }

            var paragraphs = parentNode.SelectNodes("//div/p");

            return paragraphs.ToList().Select(node => node.InnerHtml).ToList();
        }
    }
}