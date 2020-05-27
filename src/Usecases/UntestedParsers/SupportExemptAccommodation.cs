using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    class SupportExemptAccommodation : ILetterParser
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
              @".parser-lbx-ref {position:absolute;left:0;top:0;font-size:10pt}
                p.paragraph-no-margins {margin-block-start: 0; margin-block-end: 0;}
                #parser-section-one-table tr td:first-child {padding-left: 2mm;}
                #parser-section-two-table tr td:first-child {padding: 0 2mm;}

                #parser-tenant-table tr td,
                #parser-costs-table tr td,
                #parser-signature-table tr td {padding: 0 2mm;}

                #parser-costs-table tr:first-child td{height: 30mm !important;}
                #parser-costs-table tr:first-child td:last-child p {margin-block-start: 0; margin-block-end: 0;}

                #parser-tenant-table tr:nth-child(6) td {height:256mm !important;}
                #parser-tenant-table tr:nth-child(2) td:nth-child(4) p {margin-block-start: 0; margin-block-end: 0;}
                #parser-tenant-table tr:nth-child(3) td {height:26mm !important;}
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
            //page break for second page
            var secondPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Specified Accommodation status: TO BE COMPLETED BY CARE PROVIDER"));
            secondPage.Attributes.Add("style", "page-break-before: always;");

            //find section 1 table
            FindTableAndAddCustomId(secondPage.NextSibling, "parser-section-one-table");

            //find  section 2 table
            var sectionOneTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-section-one-table");
            FindTableAndAddCustomId(sectionOneTableRef.NextSibling, "parser-section-two-table");

            //reduce paragraph spacing after section 1 table
            ReduceParagraphSpacing(sectionOneTableRef, 1);

            //find tenant table
            var sectionTwoTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-section-two-table");
            FindTableAndAddCustomId(sectionTwoTableRef.NextSibling, "parser-tenant-table");

            //reduce paragraph spacing after section 1 table
            ReduceParagraphSpacing(sectionTwoTableRef, 1);

            //find costs table
            var tenantTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-tenant-table");
            FindTableAndAddCustomId(tenantTableRef.NextSibling, "parser-costs-table");

            //find signature table
            var costsTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-costs-table");
            FindTableAndAddCustomId(costsTableRef.NextSibling, "parser-signature-table");

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

            //handle out of borough details specific to this template, to be positioned in the top left corner on the header
            var lbx = $"<div class=\"parser-lbx-ref\">{table.GetRange(0, 1).FirstOrDefault()?.InnerText}</div>";

            string customAddress = lbx + table
                .GetRange(1, 7)
                .Aggregate("", (accString, node) =>
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
