using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    class ICLReminderLetter : ILetterParser
    {
        public LetterTemplate Execute(string html)
        {
            var documentNode = GetDocumentNode(html);
            var address = ParseAddressIntoLines(documentNode);

            var rightSideOfHeader = ParseSenderAddress(documentNode) + ContactDetails(documentNode);

            var mainBody = documentNode.SelectSingleNode("html/body");
            mainBody.RemoveChild(mainBody.SelectSingleNode("table[1]"));

            var templateSpecificCss = documentNode.SelectSingleNode("html/head/style").InnerText;

            //add custom table styles for print
            templateSpecificCss = templateSpecificCss.Replace("-->",
              @".header-table ~ p {margin-block-start: 0; margin-block-end: 0;}
                .header-table + p {margin-block-start: 1em; margin-block-end: 1em;}
                p.paragraph-no-margins {margin-block-start: 0; margin-block-end: 0;}
                #parser-claim-reference-table td:last-child {width: 85mm !important; word-break:break-all;}
                #parser-claim-reference-table  {page-break-before: always;}
                #parser-signature-table tr td {padding-left: 5mm !important;}
              -->");

            return new LetterTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                AddressLines = address,
                RightSideOfHeader = rightSideOfHeader,
                MainBody = AddPageBreaks(mainBody).InnerHtml,
            };
        }

        private static HtmlNode AddPageBreaks(HtmlNode body)
        {
            //page break for information page
            var informationPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Revenues &amp; Benefits Service"));
            informationPage.Attributes.Add("style", "page-break-before: always;");

            //find information table
            FindTableAndAddCustomId(informationPage, "parser-information-table");

            //find claim reference table
            var informationTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-information-table");
            FindTableAndAddCustomId(informationTableRef.NextSibling, "parser-claim-reference-table");

            //find signature table
            var claimReferenceTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-claim-reference-table");
            FindTableAndAddCustomId(claimReferenceTableRef.NextSibling, "parser-signature-table");

            return body;
        }

        private static void FindTableAndAddCustomId(HtmlNode referenceNode, string id, bool searchForward = true, bool skipFirstTable = false)
        {
            bool tableFound = false;

            while (!tableFound)
            {
                if (referenceNode.Name == "table")
                {
                    if (skipFirstTable)
                    {
                        skipFirstTable = false;
                    }
                    else
                    {
                        tableFound = true;
                        referenceNode.Attributes.Add("id", id);
                    }
                }
                else
                {
                    referenceNode = searchForward ? referenceNode.NextSibling : referenceNode.PreviousSibling;
                }
            }
        }

        private static string ParseSenderAddress(HtmlNode documentNode)
        {
            var table = documentNode.SelectNodes("html/body/table[1]/tr").ToList();
            return table
                .GetRange(1, 7)
                .Aggregate("", (accString, node) =>
                    accString + $"<p>{node.SelectNodes("td").Last().InnerHtml}</p>");
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
