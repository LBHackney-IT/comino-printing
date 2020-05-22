using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    class UCHBCTRCancelled : ILetterParser
    {
        public LetterTemplate Execute(string html)
        {
            var documentNode = GetDocumentNode(html);
            var address = ParseAddressIntoLines(documentNode);

            var rightSideOfHeader = ParseSenderAddress(documentNode) + ContactDetails(documentNode);

            var mainBody = documentNode.SelectSingleNode("html/body");
            mainBody.RemoveChild(mainBody.SelectSingleNode("table[1]"));

            var templateSpecificCss = documentNode.SelectSingleNode("html/head/style").InnerText;
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
            var firstRow = documentNode.SelectNodes("/html/body/table[1]/tr")
                .ToList().GetRange(8, 1)[0];
            var secondRow = documentNode.SelectNodes("/html/body/table[1]/tr")
                .ToList().GetRange(9, 1)[0];

            firstRow.RemoveChild(firstRow.SelectSingleNode("td[1]"));
            secondRow.RemoveChild(secondRow.SelectSingleNode("td[1]"));

            string startAcc = $"{firstRow.OuterHtml + secondRow.OuterHtml}";

            var remainingRows = documentNode.SelectNodes("/html/body/table[1]/tr")
                .ToList().GetRange(10, 6);
            return $"<table> {remainingRows.Aggregate(startAcc, (accRows, row) => accRows + row.OuterHtml)} </table>";
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
