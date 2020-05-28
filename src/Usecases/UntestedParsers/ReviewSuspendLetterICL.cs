using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    class ReviewSuspendLetterICL : ILetterParser
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
              @"#parser-signature-table tr td
                {height:10mm !important; padding-left: 5mm !important;}
                
                #parser-reply-table td {height: 5mm !important;}

                #parser-reply-table tr:nth-child(1) td:first-child,
                #parser-reply-table tr:nth-child(5) td:first-child, 
                #parser-reply-table tr:nth-child(9) td:first-child,
                #parser-reply-table tr:nth-child(13) td:first-child
                {width:5mm !important;padding-left:2mm !important;}
              
                #parser-reply-table tr:nth-child(1) td:last-child,
                #parser-reply-table tr:nth-child(5) td:last-child,
                #parser-reply-table tr:nth-child(9) td:last-child,
                #parser-reply-table tr:nth-child(13) td:last-child
                {padding-left:5mm !important;}
              
                #parser-reply-table tr:nth-child(2) td:first-child,
                #parser-reply-table tr:nth-child(6) td:first-child,
                #parser-reply-table tr:nth-child(10) td:first-child,
                #parser-reply-table tr:nth-child(14) td:first-child
                {width:5mm !important;}

                #parser-reply-table tr:last-child td{height:auto !important;}
              
                #parser-claim-reference-table {page-break-before: always;}
                #parser-claim-reference-table td:last-child {width: 85mm !important; word-break:break-all;}
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

            FindTableAndAddCustomId(informationPage, "parser-information-table");

            var informationTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-information-table");
            FindTableAndAddCustomId(informationTableRef.NextSibling, "parser-claim-reference-table");

            var declarationTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-claim-reference-table");
            FindTableAndAddCustomId(declarationTableRef.NextSibling, "parser-signature-table");

            var signatureTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-signature-table");
            FindTableAndAddCustomId(signatureTableRef.NextSibling, "parser-reply-table");

            var replyTableRef = body.ChildNodes.ToList().Find(node => node.Id == "parser-reply-table");
            FindTableAndAddCustomId(replyTableRef.NextSibling, "parser-footer-table");

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
