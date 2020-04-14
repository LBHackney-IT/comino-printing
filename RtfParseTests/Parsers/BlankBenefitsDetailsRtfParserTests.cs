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
    public class BlankBenefitsRtfParserTests
    {
        private DirectoryInfo _testResultsDirectory;
        private DirectoryInfo _testBlankBenefitsDirectory;
        private BlankBenefitsRtfParser _bbparser;

        [SetUp]
        public void Setup()
        {
            _bbparser = new BlankBenefitsRtfParser();
            _testBlankBenefitsDirectory = new DirectoryInfo("./../../../BlankBenefitsRtf");
            _testResultsDirectory = new DirectoryInfo("./../../../JSONTestResults");
        }
        
        [Test]
        public void ExecuteReturnsBlankBenefitsLetterDetails()
        {
            _testBlankBenefitsDirectory.GetFiles().ToList().ForEach(fileInfo =>
            {
                var fileName = fileInfo.Name.Split(".").First();
                var testResultsPath = _testResultsDirectory + "/" + fileName + ".json";
                var expectedResults = JsonConvert.DeserializeObject<BlankBenefitsTemplate>(
                    File.ReadAllText(testResultsPath));

                _bbparser.Execute(fileInfo).AddressFields.Should().BeEquivalentTo(expectedResults.AddressFields);
                _bbparser.Execute(fileInfo).Greeting.Should().Be(expectedResults.Greeting);
                _bbparser.Execute(fileInfo).LetterBody.Should().Be(expectedResults.LetterBody);
                _bbparser.Execute(fileInfo).LetterClosing.Should().BeEquivalentTo(expectedResults.LetterClosing);
            });
        }
    }
}