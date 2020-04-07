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

        [TestCase(0, "Claim Number 60065142")]
        [TestCase(1, "Claim Number 50314947")]
        [TestCase(2, "Claim Number 60617734")]
        public void ExecuteReturnsCorrectTitleAndClaimNumber(int fileNo, string claimNumber)
        {
            var fileInfo = _testFixtureDirectory.GetFiles().ElementAt(fileNo);
            
            _parser.Execute(fileInfo).Title.Should().Be("BENEFIT INCOME VERIFICATION DOCUMENT");
            _parser.Execute(fileInfo).ClaimNumber.Should().Be(claimNumber);
        }
        
        [TestCase(0, "Mr Richard Bruno")]
        [TestCase(1, "Mr Wentworth Blackstock")]
        [TestCase(2, "Ms Kazi Ayesha")]
        
        public void ExecuteReturnsCorrectAddresseeName(int fileNo, string name)
        {
            var fileInfo = _testFixtureDirectory.GetFiles().ElementAt(fileNo);

            _parser.Execute(fileInfo).Name.Should().Be(name);
        }
    }
}