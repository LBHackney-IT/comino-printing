using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    class AdverseInferenceBEN999 : ILetterParser
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
                #parser-declaration-table tr:last-child td{height:auto !important;}
                
                #parser-appeal-table-1 tr td {height:7mm !important;}
                #parser-appeal-table-1 td:last-child {page-break-before: always;}
                #parser-appeal-table-1 tr td:nth-child(2) {border-top:1px solid #000 !important; border-bottom: none !important;}
                #parser-appeal-table-1 tr:last-child td:nth-child(2) {border-bottom:1px solid #000 !important;}

                #parser-appeal-table-1 tr:first-child td:nth-child(2),
                #parser-appeal-table-1 tr:nth-child(2) td:nth-child(2)
                {border-top:none !important;}

                #parser-appeal-table-2 tr td {height:7mm !important;}
                #parser-appeal-table-2 td:last-child {page-break-before: always;}
                #parser-appeal-table-2 tr td:nth-child(2) {border-top:1px solid #000 !important; border-bottom: none !important;}
                #parser-appeal-table-2 tr:last-child td:nth-child(2) {border-bottom:1px solid #000 !important;}

                #parser-appeal-table-2 tr:first-child td:nth-child(2),
                #parser-appeal-table-2 tr:nth-child(2) td:nth-child(2)
                {border-top:none !important;}


                #parser-request-table-1 tr td {height:7mm !important;}
                #parser-request-table-1 td:last-child {page-break-before: always;}
                #parser-request-table-1 tr td:nth-child(2) {border-top:1px solid #000 !important; border-bottom: none !important;}
                #parser-request-table-1 tr:last-child td:nth-child(2) {border-bottom:1px solid #000 !important;}

                #parser-request-table-1 tr:first-child td:nth-child(2),
                #parser-request-table-1 tr:nth-child(2) td:nth-child(2)
                {border-top:none !important;}

                #parser-request-table-2 tr td {height:7mm !important;}
                #parser-request-table-2 td:last-child {page-break-before: always;}
                #parser-request-table-2 tr td:nth-child(2) {border-top:1px solid #000 !important; border-bottom: none !important;}
                #parser-request-table-2 tr:last-child td:nth-child(2) {border-bottom:1px solid #000 !important;}

                #parser-request-table-2 tr:first-child td:nth-child(2),
                #parser-request-table-2 tr:nth-child(2) td:nth-child(2)
                {border-top:none !important;}
                
                #parser-claim-reference-table td:last-child {width: 85mm !important; word-break:break-all;}

                p.paragraph-no-margins {margin-block-start: 0; margin-block-end: 0;}

                #parser-signature-table tr td {padding-left: 5mm !important;}
                #parser-attachments-table td:first-child p:nth-child(3) {padding-left: 5mm !important; padding-right: 5mm !important;}
                #parser-attachments-table {page-break-before: always;}

                .parser-checkbox {border: 2px solid #000; width: 25px; height: 25px; margin-top: 20px;}
              -->");

            return new LetterTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                AddressLines = address,
                RightSideOfHeader = rightSideOfHeader,
                MainBody = AddPageBreaksAndCustomIds(mainBody).InnerHtml,
            };
        }

        private static HtmlNode AddPageBreaksAndCustomIds(HtmlNode body)
        {
            //page break for information page
            var informationPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("BEN999REV-APPEAL"));
            informationPage.Attributes.Add("style", "page-break-before: always;");

            //find claim reference table and add custom id for styling
            HtmlNode claimReferencePageRefNode = informationPage.NextSibling;
            FindNextTableAndAddCustomId(claimReferencePageRefNode, "parser-claim-reference-table", true);

            //locate first request table
            var requestPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Request for a Revision of your Housing Benefit and/or Council Tax Reduction Decision"));
            FindNextTableAndAddCustomId(requestPage, "parser-request-table-1");

            //locate second request table
            var requestTableOneRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-request-table-1");
            FindNextTableAndAddCustomId(requestTableOneRef.NextSibling, "parser-request-table-2");

            //replace checkbox unicode (can sometimes appear already ticked otherwise) with simple div with borders
            var firstCheckbox = requestTableOneRef.SelectSingleNode("tr[1]/td[1]/font[1]");
            ReplaceUnicodeBoxWithDiv(firstCheckbox);

            //replace checkbox unicode (can sometimes appear already ticked otherwise) with simple div with borders
            var requestTableTwoRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-request-table-2");
            var secondCheckbox = requestTableTwoRef.SelectSingleNode("tr[1]/td[1]/font[1]");
            ReplaceUnicodeBoxWithDiv(secondCheckbox);

            //page page for appeal page
            var appealPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Request to Appeal against your Housing Benefit Decision to an Independent Tribunal"));

            //find the first appeal table and add custom id for styling
            HtmlNode declaratioNodeRef = appealPage;
            FindNextTableAndAddCustomId(declaratioNodeRef, "parser-appeal-table-1");

            //find the first appeal table and add custom id for styling
            HtmlNode appealPageRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-appeal-table-1");
            FindNextTableAndAddCustomId(appealPageRef.NextSibling, "parser-appeal-table-2", true);

            //replace checkbox unicode (can sometimes appear already ticked otherwise) with simple div with borders
            var thirdCheckbox = appealPageRef.SelectSingleNode("tr[1]/td[1]/font[1]");
            ReplaceUnicodeBoxWithDiv(thirdCheckbox);

            //find signature table
            HtmlNode appealPageRef2 = body.ChildNodes.ToList().Find(node => node.Id == "parser-appeal-table-2");
            FindNextTableAndAddCustomId(appealPageRef2.NextSibling, "parser-signature-table", true);

            var parserSignatureTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-signature-table");

            //find attachment information table
            HtmlNode signatureTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-signature-table");
            FindNextTableAndAddCustomId(signatureTableRef.NextSibling, "parser-attachments-table", true);

            HtmlNode lastPageRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-attachments-table");

            return body;
        }

        private static void ReplaceUnicodeBoxWithDiv(HtmlNode node)
        {
            //check couple of properties for safety
            if (node.Name == "font" && node.InnerHtml.Contains("&#x25A1;"))
            {
                node.Name = "div";
                node.Attributes.RemoveAll();
                node.Attributes.Add("class", "parser-checkbox");
                node.InnerHtml = "";
            }
        }

        private static void FindNextTableAndAddCustomId(HtmlNode referenceNode, string id, bool skipFirstTable = false)
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
                    referenceNode = referenceNode.NextSibling;
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
            var rows = documentNode.SelectNodes("/html/body/table[1]/tr")
                .ToList()
                .GetRange(9, 8)
                .Aggregate("", (accRows, row) => accRows + row.OuterHtml);
            return $"<table> {rows} </table>";
        }

        private static List<string> ParseAddressIntoLines(HtmlNode documentNode)
        {
            var addressList = new List<string>();
            var name = documentNode.SelectSingleNode("/html/body/table[1]/tr[8]/td[1]");
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
