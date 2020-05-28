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

            templateSpecificCss = templateSpecificCss.Replace("-->",
             @".parser-lbx-ref {position:absolute;left:0;top:0;font-size:10pt}
              -->");

            return new LetterTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                AddressLines = address,
                RightSideOfHeader = rightSideOfHeader,
                MainBody = mainBody.InnerHtml,
            };
        }

        private static string ParseSenderAddress(HtmlNode documentNode)
        {
            var table = documentNode.SelectNodes("html/body/table[1]/tr").ToList();

            //some letters have name of the borough at the top right hand corner. Handle this so it appears in the right place on print
            //get borough name that's outside the address table

            var boroughNode = documentNode.SelectNodes("html/body/p[1]").FirstOrDefault();
            string lbx = "";

            if (boroughNode != null && boroughNode.InnerText != null)
            {
                if (boroughNode.InnerText.StartsWith("London Borough of"))
                {
                    //grab the name for positioning later
                    lbx = boroughNode.InnerText;
                    //hide the initial content, this won't have any effect if the content is something else
                    boroughNode.Attributes.Add("style", "display:none");
                }
            }

            string customAddress = table
                .GetRange(1, 7)
                .Aggregate($"<div class=\"parser-lbx-ref\">{lbx}</div>", (accString, node) =>
                    accString + $"<p>{node.SelectNodes("td").Last().InnerHtml}</p>");

            return customAddress;
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
