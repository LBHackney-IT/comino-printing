using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Usecases.Domain;
using Usecases.Interfaces;

namespace Usecases.UntestedParsers
{
    class CTaxEnquiryForm : ILetterParser
    {
        public LetterTemplate Execute(string html)
        {
            var documentNode = GetDocumentNode(html);
            var address = ParseAddressIntoLines(documentNode);

            // Right side of the what's considered a header among other templates. Now it's simply instructions for the form.
            var rightSideOfHeader = ContactDetails(documentNode);

            var letterHeader = SelectTrueHeader(documentNode);

            // Remove all the <p> elements out of which the letter header was formed.
            var mainBody = GetDocumentNode(
                Regex.Replace(
                    documentNode.OuterHtml,
                    @"(?<=<body>).+?(?=<table[^<]*>)",
                    "",
                    RegexOptions.Singleline
                )).SelectSingleNode("html/body");

            // Remove the address table as it was extracted into its own element. If not removed, it would be repeated twice.
            mainBody.RemoveChild(mainBody.SelectSingleNode("table[1]"));

            // Fix the layout of the form questions. Each question is pretty much a table.
            FixMainBodyTables(mainBody);

            // Add page break for non-form content
            var additionalInfoPage = mainBody.SelectNodes("p").ToList().FirstOrDefault(p => p.InnerText.Contains("Information for Students"));
            AddAppendAttribute(additionalInfoPage, "style", "; page-break-before: always;");

            var templateSpecificCss = documentNode.SelectSingleNode("html/head/style").InnerText;

            // Ratios to tweak the content faster.
            var headerRatio = 0.7;
            var rightTableRatio = 0.6;

            /*
                The following CSS changes are in rough order:

                - Make pre-form legal info (header-table) scale with its child elements by the same proportion.
                - Make letter header (#letter-header) scale with its child elements by the same proportion.
                - Override the general CSS for element (mm) positioning (adress, header-table) as the existence of letter header changes the layout
                  dramatically. New values were recalculated out of GovNotify letter template example just as the original general CSS was.
                - Added the hard @page margins as they were not in general CSS, making some margins too small (after removing default margins), and some too large when comparing to 
                  GovNotify required format.
                - Remove default body margins.
                - Reduce main body table cell (td) heights, and reduce row (tr) sizes by removing the default unexplainable height increase, by adding font-size:0;
                - Add padding to some table cells, where the text was touching the border.
                - Add page break before question 6, as it wouldn't have fit inside the page.
            */
            templateSpecificCss = templateSpecificCss.Replace("-->",
            $@".header-table ~ p {{margin-block-start: 0; margin-block-end: 0; margin: 0}}
              .header-table + p {{margin-block-start: 1em; margin-block-end: 1em; margin: 0}}
              .header-right font.c6, .header-right font.c10 {{font-size: {(int)Math.Floor(10 * rightTableRatio)}pt}}
              /*.header-right p {{margin-block-start: 0; margin-block-end: 0; margin: 0;}}*/
              #letter-header > p {{margin-block-start: 0; margin-block-end: 0; margin: 0; font-size: 0}}
              #letter-header p font.c1, #letter-header p font.c3 {{font-size: {(int)Math.Floor(22 * headerRatio)}pt}}
              #letter-header p font.c2, #letter-header p font.c4, #letter-header p font.c5, #letter-header p font.c6 {{font-size: {(int)Math.Floor(11 * headerRatio)}pt}}
              #letter-header p font.c7 {{font-size: {(int)Math.Floor(16 * headerRatio)}pt}}
              #letter-header p font.c8 {{font-size: {(int)Math.Floor(8 * headerRatio)}pt}}
              font.c2 > img {{
                width: {(int)Math.Floor(38 * headerRatio)}px;
                height: {(int)Math.Floor(31 * headerRatio)}px;
              }}
              .header-right {{
                    min-height: 65mm;
              }}
              #letter-header {{
                    height: 25mm;
                    padding-left: 9.6mm;
              }}
              .header-table,
              .header-table tr td:nth-child(1),
              .header-table tr td:nth-child(2) {{
                  min-height: 65mm;
              }}
              #address {{
                    left: 12.6mm;
              }}
              @page {{
                  margin-top: 5mm;
                  margin-bottom: 5mm;
                  margin-left: 15mm;
                  margin-right: 15mm;    
              }}
              body {{margin-top: 0}}

              table#question-1 tr {{
                    font-size: 0;
              }}

              table#question-3 tr {{
                  font-size: 0;
              }}

              table#question-3 tr td {{
                  height: 20px !Important;
              }}

              #question-6 {{
                    page-break-before: always;
              }}
              #question-7b td {{
                    padding: 0 5px 0 10px !Important;
              }}
              #student-info p {{
                    font-size:0;
              }}
              #student-info tr:first-child td {{
                    padding: 5px 0 0 10px !Important;
                    height: 25px !Important;
              }}
              #student-info tr:not(:first-child) td:nth-child(3) {{
                    padding: 0 10px 0 5px !Important;
              }}
              #other-info td {{
                    padding: 0 5px 0 10px !Important;
              }}
                 -- >");


            return new LetterTemplate
            {
                TemplateSpecificCss = templateSpecificCss,
                AddressLines = address,
                RightSideOfHeader = rightSideOfHeader,
                MainBody = mainBody.OuterHtml,
                LetterHead = letterHeader
            };
        }

        private static bool ContainsBorder(HtmlNode node)
        {
            return node.Attributes.FirstOrDefault(a => a.Name == "style")?.Value.Contains("border") ?? false;
        }

        private static bool HtmlAttrNull(HtmlNode node, string attr_name)
        {
            return node.Attributes.FirstOrDefault(a => a.Name == attr_name) == null ? true : false;
        }

        private static void AddAppendAttribute(HtmlNode node, string attributeName, string value)
        {
            var nodeAttribute = node.Attributes.FirstOrDefault(a => a.Name == attributeName);
            if (nodeAttribute != null) nodeAttribute.Value += $" {value}"; else node.Attributes.Add(attributeName, value);
        }

        private static void ChangeAttributeIfExists(HtmlNode node, string attributeName, string value)
        {
            var nodeAttribute = node.Attributes.FirstOrDefault(a => a.Name == attributeName);
            if (nodeAttribute != null) nodeAttribute.Value = value;
        }

        private static void FixMainBodyTables(HtmlNode mainLetterBody)
        {
            var questionTables = mainLetterBody.SelectNodes("table").ToList();

            foreach (var table in questionTables)
            {
                var questionNumberMatch = Regex.Match(table.InnerHtml, @"(?<=>)\d(?=\. ?<\/font>)");
                var questionRows = table.SelectNodes("tr").ToList();

                if (questionNumberMatch.Success)
                {
                    AddAppendAttribute(table, "id", $"question-{questionNumberMatch.Value}");
                    switch (questionNumberMatch.Value)
                    {
                        case "2":
                            questionRows.ForEach(row => {
                                var cols = row.SelectNodes("td").ToList();

                                if (cols.Count.Equals(5)) // Borders between table rows contain pointless rows with less than 5 columns
                                    cols.ForEach(col => {
                                        if (ContainsBorder(col))
                                            col.InnerHtml = "&nbsp;"; // The table cells have no height unless you add content.
                                    });
                            });
                            break;
                        case "3":
                            HtmlNode faultyRowspanRow = null;
                            foreach (var (row, r_index) in questionRows.Select((r, i) => (r, i)))
                            {
                                var cols = row.SelectNodes("td").ToList();

                                foreach (var (col, c_index) in cols.Select((c, i) => (c, i)))
                                {
                                    ChangeAttributeIfExists(col, "rowspan", "1");

                                    if (col.InnerText.Contains("Date purchased/rented/leased:"))
                                        AddAppendAttribute(col, "style", "border-bottom-style: solid; border-bottom-width: 1pt; ");


                                    if (cols.Count.Equals(3) && col.InnerText.Contains("Student? (Yes / No)") && ContainsBorder(cols[c_index + 1]))
                                    {
                                        var nextRowCols = questionRows[r_index + 1].ChildNodes;

                                        nextRowCols.Insert(nextRowCols.Count - 2, col);
                                        nextRowCols.Insert(nextRowCols.Count - 2, cols[c_index + 1]);

                                        faultyRowspanRow = row;
                                    }
                                }
                            }
                            faultyRowspanRow.Remove();
                            break;
                        case "4":
                            questionRows
                                .ForEach(row => row.SelectNodes("td").ToList()
                                .ForEach(col => {
                                    if (ContainsBorder(col) || !HtmlAttrNull(col, "class"))
                                        AddAppendAttribute(col, "style", "; padding: 0 10px 0 5px !Important;");
                                }));
                            break;
                    }
                }
                else if (table.InnerHtml.Contains("I DECLARE THAT THE INFORMATION PROVIDED ON THIS FORM IS CORRECT TO THE BEST OF MY KNOWLEDGE"))
                {
                    AddAppendAttribute(table, "id", "question-7b");

                    questionRows
                        .ForEach(row => row.SelectNodes("td").ToList()
                        .ForEach(col => {
                            if (ContainsBorder(col) || !HtmlAttrNull(col, "class"))
                                AddAppendAttribute(col, "style", "; padding: 0 10px 0 5px !Important;");
                        }));
                }
                else if (table.InnerHtml.Contains("A Student is:"))
                {
                    AddAppendAttribute(table, "id", "student-info");
                }
                else if (table.InnerHtml.Contains("The following may be eligible for discounts/exemptions:"))
                {
                    AddAppendAttribute(table, "id", "other-info");
                }
                else
                {
                    AddAppendAttribute(table, "id", "something-new-was-added");
                }
            }
        }

        private static string SelectTrueHeader(HtmlNode documentNode)
        {
            var initialParagraphs = documentNode.SelectNodes("html/body/p").ToList().GetRange(1, 10);

            var header = $@"
                <div id='letter-header'>
                  {initialParagraphs.Aggregate("", (acc, paragraph) => acc + paragraph.OuterHtml + '\n')}
                </div>
            ";

            header = Regex.Replace(header, @"<p align=""\w+"">&nbsp;</p>", "");

            return header;
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
            var dateColumns = documentNode.SelectNodes("/html/body/table[1]/tr[1]/td").ToList().GetRange(1, 2);
            var dateRow = HtmlNode.CreateNode($"<tr> {dateColumns.Aggregate("", (acc, col) => acc + col.OuterHtml + "\n")} </tr>");

            var rightTableRows = documentNode.SelectNodes("/html/body/table[1]/tr").ToList().GetRange(1, 3);

            rightTableRows.Insert(0, dateRow);

            return $@"<table id=""right-side-table""> {rightTableRows.Aggregate("", (acc, row) => acc + row.OuterHtml)} </table>";
        }

        private static List<string> ParseAddressIntoLines(HtmlNode documentNode)
        {
            var addressList = new List<string>();

            documentNode.SelectSingleNode("/html/body/table[1]/tr[1]/td[1]").ChildNodes.ToList()
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
