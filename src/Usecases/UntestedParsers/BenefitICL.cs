using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    public class BenefitICL : ILetterParser
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
                #parser-claim-reference-table + p {margin-block-start: 0; margin-block-end: 0;}
              
                #parser-claim-reference-table {width: 100%;}
                #parser-claim-reference-table td:last-child {width: 85mm !important; word-break:break-all;}
                p.paragraph-no-margins {margin-block-start: 0; margin-block-end: 0;}
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
            var informationPage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("Revenues &amp; Benefits Service"));
            informationPage.Attributes.Add("style", "page-break-before: always;");

            //page break for claim reference page
            var claimReferencePage = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("You MUST enclose this page with your reply otherwise it could delay processing your claim"));
            claimReferencePage.Attributes.Add("style", "page-break-before: always;margin-block-start: 0; margin-block-end: 0;");

            //locate declaration block
            var declaratioNode = body.ChildNodes.ToList().Find(node => node.InnerText.Contains("DECLARATION"));
            declaratioNode.Attributes.Add("style", "margin-block-start: 0;");

            //remove additional spaces to improve page flow 
            HtmlNode sibling = declaratioNode.NextSibling;

            int count = 0;

            while (count < 6)
            {
                if (sibling.Name == "p")
                {
                    sibling.Attributes.Add("class", "paragraph-no-margins");
                    count++;
                }
                sibling = sibling.NextSibling;
            };

            //find claim reference table and add custom id for styling
            HtmlNode claimReferencePageRefNode = claimReferencePage;
            FindNextTableAndAddCustomId(claimReferencePageRefNode, "parser-claim-reference-table");

            //find signature table and add custom id for styling
            HtmlNode declaratioNodeRef = declaratioNode;
            FindNextTableAndAddCustomId(declaratioNodeRef, "parser-signature-table");

            //get the last table (declaration) and add custom id
            var lastTable = body.SelectNodes("table").Last();
            lastTable.Attributes.Add("id", "parser-declaration-table");

            return body;
        }

        private static void FindNextTableAndAddCustomId(HtmlNode referenceNode, string id)
        {
            bool tableFound = false;

            while (!tableFound)
            {
                if (referenceNode.Name == "table")
                {
                    tableFound = true;
                    referenceNode.Attributes.Add("id", id);
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
                .GetRange(1, 6)
                .Aggregate("", (accString, node) =>
                    accString + $"<p>{node.SelectNodes("td").Last().InnerHtml}</p>");
        }

        private static string ContactDetails(HtmlNode documentNode)
        {
            var phoneNumber = documentNode.SelectSingleNode("/html/body/table[1]/tr[9]");
            phoneNumber.ChildNodes.RemoveAt(1);

            var emailAddress = documentNode.SelectSingleNode("/html/body/table[1]/tr[10]");
            emailAddress.ChildNodes.Remove(1);

            var restOfTheContactDetails = documentNode.SelectNodes("/html/body/table[1]/tr").ToList().GetRange(10, 6)
                .Aggregate("", (accRows, row) => accRows + row.OuterHtml);

            var output = $"<table>{phoneNumber.OuterHtml}{emailAddress.OuterHtml}{restOfTheContactDetails}</table>";

            return output;
        }

        private static List<string> ParseAddressIntoLines(HtmlNode documentNode)
        {
            var addressList = new List<string>();
            var name = documentNode.SelectSingleNode("/html/body/table[1]/tr[9]/td[1]");
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
