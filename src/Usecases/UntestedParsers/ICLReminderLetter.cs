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
              @"p.paragraph-no-margins {margin-block-start: 0; margin-block-end: 0;}
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

            //reduce paragraph spacing on information page
            ReduceParagraphSpacing(informationPage, 21);

            //find claim reference table
            var informationTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-information-table");
            FindTableAndAddCustomId(informationTableRef.NextSibling, "parser-claim-reference-table");

            //reduce paragraph spacing after information table to adjust page break location
            ReduceParagraphSpacing(informationTableRef, 6);

            //find signature table
            var claimReferenceTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-claim-reference-table");
            FindTableAndAddCustomId(claimReferenceTableRef.NextSibling, "parser-signature-table");

            //reduce paragraph spacing on last page
            var signatureTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-signature-table");
            ReduceParagraphSpacing(signatureTableRef, 21);

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

        private static void ReduceParagraphSpacing(HtmlNode referenceNode, int paragraphCount)
        {
            int count = 0;

            while (count < paragraphCount)
            {
                if (referenceNode == null) break;

                if (referenceNode.Name == "p")
                {
                    referenceNode.Attributes.Add("class", "paragraph-no-margins");
                    count++;
                }
                referenceNode = referenceNode.NextSibling;
            };
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
