using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers.CouncilTax
{
    class OccupierLetterToOccReqOwnerDetails : ILetterParser
    {
        public LetterTemplate Execute(string html)
        {
            var documentNode = GetDocumentNode(html);
            var address = ParseAddressIntoLines(documentNode);

            var rightSideOfHeader = ParseSenderAddress(documentNode) + ContactDetails(documentNode);

            var mainBody = documentNode.SelectSingleNode("html/body");
            mainBody.RemoveChild(mainBody.SelectSingleNode("table[1]"));

            var newPageText = mainBody.SelectNodes("p").ToList().FirstOrDefault(p => p.InnerText.Contains("Letter to Occupier requesting Owner Details"));
            newPageText.Attributes.Add("id", "new-page");

            var templateSpecificCss = documentNode.SelectSingleNode("html/head/style").InnerText;

            templateSpecificCss = templateSpecificCss.Replace("-->",
              @".header-table ~ p {margin-block-start: 0; margin-block-end: 0;}
                .header-table + p {margin-block-start: 1em; margin-block-end: 1em;}
                #new-page {page-break-before: always;}
              -->");

            return new LetterTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                AddressLines = address,
                RightSideOfHeader = rightSideOfHeader,
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

        private static List<string> ParseAddressIntoLines(HtmlNode documentNode)
        {
            var addressList = new List<string>();
            var name = documentNode.SelectSingleNode("/html/body/table/tr[9]/td[1]");

            addressList.Add(name.InnerText);

            documentNode.SelectSingleNode("/html/body/table[1]/tr[10]/td[1]").ChildNodes.ToList()
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
