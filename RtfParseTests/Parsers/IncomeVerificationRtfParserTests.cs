using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RtfParseSpike.Parsers;
using RtfParseSpike.Templates;

namespace RtfParseTests
{
    public class IncomeVerificationRtfParserTests
    {
        private DirectoryInfo _testFixtureDirectory;
        private IncomeVerificationRtfParser _parser;

        [SetUp]
        public void Setup()
        {
            _parser = new IncomeVerificationRtfParser();
            _testFixtureDirectory = new DirectoryInfo("./../../../ExampleLettersHtml");
        }

        [TestCase(0, "60065142")]
        [TestCase(1, "50314947")]
        public void ExecuteReturnsCorrectTitleAndClaimNumber(int fileNo, string claimNumber)
        {
            var fileInfo = _testFixtureDirectory.GetFiles().ElementAt(fileNo);

            _parser.Execute(fileInfo).Should().BeEquivalentTo(new IncomeVerificationTemplate
            {
                Title = "BENEFIT INCOME VERIFICATION DOCUMENT",
                ClaimNumber = claimNumber
            });
        }
    }
}