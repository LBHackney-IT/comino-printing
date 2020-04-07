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

        [Test]
        public void ExecuteReturnsCorrectTitleAndClaimNumber()
        {
            var fileInfo = _testFixtureDirectory.GetFiles().FirstOrDefault();

            _parser.Execute(fileInfo).Should().BeEquivalentTo(new IncomeVerificationTemplate
            {
                Title = "BENEFIT INCOME VERIFICATION DOCUMENT",
                ClaimNumber = "60065142"
            });
        }
    }
}