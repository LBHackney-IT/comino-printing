using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Usecases;
using Usecases.UntestedParsers;

namespace UnitTests
{
    public class ParsingHelpersTests
    {
        [Test]
        public void FormatHeaderReturnsFormattedTableIncludingTheAddress()
        {
            var addressLines = new List<string>
            {
                "Name",
                "Address Line 1",
                "Address Line 2",
                "City",
                "County",
                "Postcode"
            };

            var expectedAddressString = @"Name<br />Address Line 1<br />Address Line 2<br />City<br />County<br />Postcode";

            var result = ParsingHelpers.FormatLetterHeader(addressLines, "");
            result.Should().Contain(expectedAddressString);
        }

        [Test]
        public void FormatHeaderReturnsFormattedTableIncludingTheRightSideOfHeader()
        {
            var rightSideOfHeader = "<p>Lots of html and CSS</p>";

            var result = ParsingHelpers.FormatLetterHeader(new List<string>{"Name"}, rightSideOfHeader);
            result.Should().Contain(rightSideOfHeader);
        }

        [Test]
        public void FormatHeaderStripsWhitespaceFromAddressLines()
        {
            var addressLines = new List<string>
            {
                "Name",
                "      ",
                "Address Line 1",
                "        ",
                "Address Line 2",
                "\n",
                "City",
                "\n      ",
                "County",
                "Postcode"
            };

            var expectedAddressString = @"Name<br />Address Line 1<br />Address Line 2<br />City<br />County<br />Postcode";

            var result = ParsingHelpers.FormatLetterHeader(addressLines, "");
            result.Should().Contain(expectedAddressString);
        }
    }
}