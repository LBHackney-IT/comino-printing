using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    class BenefitDefectiveNewClaim : ILetterParser
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

                #parser-signature-table {page-break-after: always;}
                #parser-signature-table tr td {padding-left: 5mm !important;}

                #parser-address-table td {padding: 0 5mm;}

                /*fix weird font tags*/
                p.zero-font-size {font-size: 0px !important;}
                p.zero-font-size.font-fix-in-statement font {font-family: sans-serif;font-size: 12pt;display: inline-block; padding-left: 2pt;}
                p.zero-font-size.font-fix-in-statement font:nth-child(4),
                p.zero-font-size.font-fix-in-statement font:nth-child(6){
                padding-left: 0;}

                #parser-request-table-1 tr:first-child td:nth-child(2) p:first-child{font-size: 0px !important;padding-top:12pt;}
                #parser-request-table-1 tr:first-child td:nth-child(2) p font{font-family: sans-serif;font-size: 12pt}

                #parser-request-table-2 tr:first-child td:nth-child(2) p:first-child{font-size: 0px !important;padding-top:12pt;}
                #parser-request-table-2 tr:first-child td:nth-child(2) p font{font-family: sans-serif;font-size: 12pt}

                #parser-appeal-table-1 tr:first-child td:nth-child(2) {font-size: 0px !important;}
                #parser-appeal-table-1 tr:first-child td:nth-child(2)  font{font-family: sans-serif;font-size: 12pt}
                #parser-appeal-table-1 tr:first-child td:nth-child(2)  font:nth-child(6) {padding-left:2pt;}

                .parser-checkbox {
                    border: 2px solid #000;
                    width: 25px;
                    height: 25px;
                    margin-top: 20px;
                }
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
            //find claim reference table
            var requestPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Request for a Revision of your Housing Benefit and/or Council Tax Reduction Decision"));
            FindTableAndAddCustomId(requestPage.PreviousSibling, "parser-claim-reference-table", false);

            //fix dodgy font spacing
            var statementRef = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("(Tick"));
            statementRef.Attributes.Add("class", "zero-font-size font-fix-in-statement");

            //find the first appeal table and add custom id for styling
            var claimReferenceTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-claim-reference-table");
            FindTableAndAddCustomId(claimReferenceTableRef.NextSibling, "parser-request-table-1");

            //find the second appeal table and add custom id for styling
            HtmlNode appealPageRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-request-table-1");
            FindTableAndAddCustomId(appealPageRef.NextSibling, "parser-request-table-2", true, true);

            //replace checkbox unicode (can sometimes appear already ticked otherwise) with simple div with borders
            var firstCheckbox = appealPageRef.SelectSingleNode("tr[1]/td[1]/font[1]");
            ReplaceUnicodeBoxWithDiv(firstCheckbox);

            //add page break between forms
            var appealPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Request to Appeal against your Housing Benefit Decision to an Independent Tribunal."));
            appealPage.Attributes.Add("style", "page-break-before: always;");

            //find the first appeal table and add custom id for styling
            var requestTableTwoRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-request-table-2"); ;
            FindTableAndAddCustomId(requestTableTwoRef.NextSibling, "parser-appeal-table-1", true, true);

            //replace checkbox unicode (can sometimes appear already ticked otherwise) with simple div with borders
            var secondCheckbox = requestTableTwoRef.SelectSingleNode("tr[1]/td[1]/font[1]");
            ReplaceUnicodeBoxWithDiv(secondCheckbox);

            //find the second appeal table and add custom id for styling
            HtmlNode appealTableOneRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-appeal-table-1");
            FindTableAndAddCustomId(appealTableOneRef.NextSibling, "parser-appeal-table-2", true, true);

            //replace checkbox unicode (can sometimes appear already ticked otherwise) with simple div with borders
            var thirdCheckbox = appealTableOneRef.SelectSingleNode("tr[1]/td[1]/font[1]");
            ReplaceUnicodeBoxWithDiv(thirdCheckbox);

            HtmlNode appealPageRef2 = body.ChildNodes.ToList().Find(node => node.Id == "parser-appeal-table-2");
            FindTableAndAddCustomId(appealPageRef2.NextSibling, "parser-signature-table", true, true);

            //find address table
            HtmlNode signatureTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-signature-table");
            FindTableAndAddCustomId(signatureTableRef.NextSibling, "parser-address-table", true, true);

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
