using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace Usecases
{
    public static class ParsingHelpers
    {
        public static string FormatLetterHeader(List<string> addressLines, string rightSideHeader)
        {
            var address = CompileAddressHtml(addressLines);

            return "<table class=\"header-table\">" +
                       "<tr>" +
                           "<td>" +
                               "<div id=\"address\">" + address + "</ div>" +
                           "</td>" +
                           "<td>" +
                               "<div class=\"header-right\">" + rightSideHeader + "</div>" +
                           "</td>" +
                       "</tr>" +
                   "</table>";
        }

        private static string CompileAddressHtml(List<string> addressLines)
        {
            var strippedAddressLines = StripEmptyLines(addressLines);
            var formattedLines = CompressExtraLines(strippedAddressLines);
            return string.Join("<br />", formattedLines);
        }

        private static List<string> CompressExtraLines(List<string> lines)
        {
            var numberOfLines = lines.Count;
            if (numberOfLines <= 7) return lines;
            var halfExtraLines = (numberOfLines - 5) / 2;

            return new List<string>
            {
                lines.ElementAt(0),
                lines.ElementAt(1),
                string.Join(", ", lines.GetRange(2, halfExtraLines)),
                string.Join(", ", lines.GetRange(halfExtraLines + 2, numberOfLines - 7)),
                lines.ElementAt(numberOfLines - 3),
                lines.ElementAt(numberOfLines - 2),
                lines.ElementAt(numberOfLines - 1)
            };
        }

        private static List<string> StripEmptyLines(List<string> addressLines)
        {
            return addressLines.Select(line => line.Trim()).Where(line => line != "").ToList();
        }

        //TODO: handle configuration in its own class/service
        public static DocumentConfig GetDocumentConfig()
        {
            return JsonConvert.DeserializeObject<DocumentConfig>(
                Environment.GetEnvironmentVariable("DOCUMENT_CONFIG")
            );
        }
    }
}