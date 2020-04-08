using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using RtfParseSpike.Parsers;
using RtfParseSpike.Templates;

namespace RtfParseTests
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
            _testFixtureDirectory = new DirectoryInfo("./../../../ExampleLettersRtf");
            _testResultsDirectory = new DirectoryInfo("./../../../JSONTestResults");
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
            });
        }
    }
}