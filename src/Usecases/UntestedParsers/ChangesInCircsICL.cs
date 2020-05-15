using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    public class ChangesInCircsICL : ILetterParser
    {
        public LetterTemplate Execute(string html)
        {
            var documentNode = GetDocumentNode(html);
            var address = ParseAddressIntoLines(documentNode);

            var rightSideOfHeader = ParseSenderAddress(documentNode) + ContactDetails(documentNode);

            var mainBody = documentNode.SelectSingleNode("html/body");
            mainBody.RemoveChild(mainBody.SelectSingleNode("table[1]"));

            var templateSpecificCss = documentNode.SelectSingleNode("html/head/style").InnerText;

            //add custom styles for print
            //1. fix checkbox sizes on the last page and ensure the lines are of equal height
            //2. reduce the height of signature table
            //3. make sure that content that has to fit on one page is laid out properly and don't overflow to next page 
            templateSpecificCss = templateSpecificCss.Replace("-->",
              @"#parser-signature-table tr td
                {height:10mm !important; padding-left: 5mm !important;}
                
                #parser-declaration-table td {height: 5mm !important;}

                #parser-declaration-table tr:nth-child(1) td:first-child,
                #parser-declaration-table tr:nth-child(5) td:first-child, 
                #parser-declaration-table tr:nth-child(9) td:first-child,
                #parser-declaration-table tr:nth-child(13) td:first-child
                {width:5mm !important;padding-left:2mm !important;}
              
                #parser-declaration-table tr:nth-child(1) td:last-child,
                #parser-declaration-table tr:nth-child(5) td:last-child,
                #parser-declaration-table tr:nth-child(9) td:last-child,
                #parser-declaration-table tr:nth-child(13) td:last-child
                {padding-left:5mm !important;}
              
                #parser-declaration-table tr:nth-child(2) td:first-child,
                #parser-declaration-table tr:nth-child(6) td:first-child,
                #parser-declaration-table tr:nth-child(10) td:first-child,
                #parser-declaration-table tr:nth-child(14) td:first-child
                {width:5mm !important;}

                #parser-declaration-table tr:last-child td{height:auto !important;}
              
                #parser-claim-reference-table {page-break-before: always;}
                #parser-claim-reference-table td:last-child {width: 85mm !important; word-break:break-all;};

              -->");

            return new LetterTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                AddressLines = address,
                RightSideOfHeader = rightSideOfHeader,
                MainBody = AddPageBreaks(mainBody).OuterHtml,
            };
        }

        private static HtmlNode AddPageBreaks(HtmlNode body)
        {
            //page break for information page
            var informationPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Revenues &amp; Benefits Service"));
            informationPage.Attributes.Add("style", "page-break-before: always;");

            //get the claim reference table and add custom id
            var claimRefenceTable = body.SelectSingleNode("table[2]");
            claimRefenceTable.Attributes.Add("id", "parser-claim-reference-table");

            //get the signature table and add custom id
            var signatureTable = body.SelectSingleNode("table[3]");
            signatureTable.Attributes.Add("id", "parser-signature-table");

            //get the declaration table and add custom id
            var lastTable = body.SelectNodes("table").Last();
            lastTable.Attributes.Add("id", "parser-declaration-table");

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