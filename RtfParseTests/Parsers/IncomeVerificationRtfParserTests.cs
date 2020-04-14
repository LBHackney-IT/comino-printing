using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using RtfParseSpike.Parsers;
using RtfParseSpike.Templates;

namespace RtfParseTests.Parsers
{
    public class IncomeVerificationRtfParserTests
    {
        private DirectoryInfo _testFixtureDirectory;
        private IncomeVerificationRtfParser _parser;
        private DirectoryInfo _testResultsDirectory;

        [SetUp]
        public void Setup()
        {
            _parser = new IncomeVerificationRtfParser();
            _testFixtureDirectory = new DirectoryInfo("./../../../TestFixtures/IncomeVerification/ExampleLettersRtf");
            _testResultsDirectory = new DirectoryInfo("./../../../TestFixtures/IncomeVerification/JSONTestResults");
        }

        [Test]
        public void ExecuteReturnsCorrectTitleAndClaimNumber()
        {
            _testFixtureDirectory.GetFiles().ToList().ForEach(fileInfo =>
            {
                var fileName = fileInfo.Name.Split(".").First();
                var testResultsPath = _testResultsDirectory + "/" + fileName + ".json";
                var expectedResults = JsonConvert.DeserializeObject<IncomeVerificationTemplate>(
                    File.ReadAllText(testResultsPath));

                _parser.Execute(fileInfo).Title.Should().Be(expectedResults.Title);
                _parser.Execute(fileInfo).ClaimNumber.Should().Be(expectedResults.ClaimNumber);
                _parser.Execute(fileInfo).Name.Should().Be(expectedResults.Name);
                _parser.Execute(fileInfo).Address.Should().BeEquivalentTo(expectedResults.Address);
                _parser.Execute(fileInfo).Postcode.Should().Be(expectedResults.Postcode);
                _parser.Execute(fileInfo).PeriodStartDateTable.Should().BeEquivalentTo(expectedResults.PeriodStartDateTable);
                _parser.Execute(fileInfo).ReasonForAssessmentTable.Should().BeEquivalentTo(expectedResults.ReasonForAssessmentTable);
                _parser.Execute(fileInfo).IncomeDetailsTable.Should().BeEquivalentTo(expectedResults.IncomeDetailsTable);
                _parser.Execute(fileInfo).OverpaymentTableHeaders.Should().Be(expectedResults.OverpaymentTableHeaders);
                _parser.Execute(fileInfo).OverpaymentTable.Should().BeEquivalentTo(expectedResults.OverpaymentTable);
                _parser.Execute(fileInfo).AdditionalCommentTable.Should().BeEquivalentTo(expectedResults.AdditionalCommentTable);
            });
        }
    }
}