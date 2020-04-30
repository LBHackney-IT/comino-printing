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
            LambdaLogger.Log(JsonConvert.SerializeObject(addressLines));
            var strippedAddressLines = addressLines.Select(line => line.Trim()).Where(line => line != "");
            var address = string.Join("<br />", strippedAddressLines);
            var addressTable = "<div id=\"address\">" + address + "</ div>";
            var rightHandHeaderSpace = "<div class=\"header-right\">"+ rightSideHeader +"</div>";

            return "<table class=\"header-table\">" +
                   "<tr>" +
                   "<td>" + addressTable + "</td>" +
                   "<td>" + rightHandHeaderSpace + "</td>" +
                   "</tr>" +
                   "</table>";
        }
    }
}