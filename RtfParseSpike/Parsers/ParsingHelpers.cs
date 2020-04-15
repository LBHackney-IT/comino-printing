namespace RtfParseSpike.Parsers
{
    public static class ParsingHelpers
    {
        public static string FormatLetterHeader(string address, string rightSideHeader)
        {
            var addressTable = "<table class=\"address-table\" >" +
                               "<col width=\"9.6mm\" />" +
                               "<col width=\"95.4mm\" />" +
                               "<col width=\"5mm\" />" +
                               "<tr> <td>&nbsp;</td> <td>&nbsp;</td> <td>&nbsp;</td> </tr>" +
                               "<tr> <td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>" +
                               "<tr><td>&nbsp;</td>" +
                               "<td>" +
                               address +
                               "</td>" +
                               "<td></td></tr>" +
                               "<tr ><td></td><td></td><td></td></tr>" +
                               "</table>";
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