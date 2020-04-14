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
    public class BlankBenefitsRtfParser
    {
        public BlankBenefitsTemplate Execute(FileInfo fileInfo)
        {
            var html = GetHTMLFromFilePath(fileInfo);
            var documentNode = GetDocumentNode(html);

            var addressFields = documentNode
                .SelectNodes("/div/table/tr")
                .Select(row => row.ChildNodes
                    .Select(EditHtmlStringBasedOnTags).ToList()).ToList();

            var greeting = documentNode.SelectSingleNode("/div/p[4]").InnerText;

            var letterBody = documentNode
                .SelectNodes("/div/p")
                .ToList()
                .GetRange(4, 38)
                .Select(EditHtmlStringBasedOnTags).ToList();
            
            var letterBodyString = String.Join("\n", letterBody);

            var letterClosing = documentNode.SelectNodes("/div/p").ToList().GetRange(42, 5).Select(EditHtmlStringBasedOnTags);
            
            var letterClosingString = String.Join("\n",letterClosing );

            return new BlankBenefitsTemplate
            {
                AddressFields = addressFields,
                Greeting = greeting,
                LetterBody = letterBodyString,
                LetterClosing = letterClosingString
            };
        }
        
        private static string GetHTMLFromFilePath(FileInfo fileInfo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var letter = File.ReadAllText(fileInfo.ToString(), Encoding.UTF8);
            return Rtf.ToHtml(letter);
        }
        
        private static string EditHtmlStringBasedOnTags(HtmlNode html)
        {
            return html.ChildNodes.Aggregate("", (agg, node) =>
            {
                switch (node.Name)
                {
                    case "p":
                        return agg + "\n" + node.InnerText + "\n";
                    case "strong":
                        return agg + node.ParentNode.InnerHtml;
                    default:
                        return agg + node.InnerHtml;
                }
            }).TrimEnd('\n');
        }

        private static HtmlNode GetDocumentNode(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }
    }
}