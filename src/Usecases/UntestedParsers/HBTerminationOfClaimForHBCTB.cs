﻿using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    class HBTerminationOfClaimForHBCTB : ILetterParser
    {
        public LetterTemplate Execute(string html)
        {
            var documentNode = GetDocumentNode(html);
            var address = ParseAddressIntoLines(documentNode);

            var rightSideOfHeader = ParseSenderAddress(documentNode) + ContactDetails(documentNode);

            var mainBody = documentNode.SelectSingleNode("html/body");
            mainBody.RemoveChild(mainBody.SelectSingleNode("table[1]"));

            var templateSpecificCss = documentNode.SelectSingleNode("html/head/style").InnerText;

            templateSpecificCss = templateSpecificCss.Replace("-->",
             @".header-table ~ p {margin-block-start: 0; margin-block-end: 0;}
               .header-table + p {margin-block-start: 1em; margin-block-end: 1em;}
               .warning-header {color: red;}
              -->");

            return new LetterTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                AddressLines = address,
                RightSideOfHeader = rightSideOfHeader,
                MainBody = FormatContent(mainBody).InnerHtml
            };
        }

        private static HtmlNode FormatContent(HtmlNode body)
        {
            //use CSS instead of font color tag to ensure it doesn't "leak" to therest of the page
            var mainHeading = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Termination of claim for"));
            var fontTag = mainHeading.SelectSingleNode("font");
            fontTag.Attributes.Remove("color");
            mainHeading.Attributes.Add("class", "warning-header");

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
                .ToList().GetRange(9, 8);
            return $"<table> {rows.Aggregate("", (accRows, row) => accRows + row.OuterHtml)} </table>";
        }

        private static List<string> ParseAddressIntoLines(HtmlNode documentNode)
        {
            var addressList = new List<string>();
            var name = documentNode.SelectSingleNode("/html/body/table/tr[8]/td[1]");

            addressList.Add(name.InnerText);

            documentNode.SelectSingleNode("/html/body/table[1]/tr[9]/td[1]").ChildNodes.ToList()
                .ForEach(line => addressList.Add(line.InnerText));

            return addressList;
        }

        private static HtmlNode GetDocumentNode(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }
    }
}
