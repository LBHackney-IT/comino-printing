using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using RtfParseSpike.Parsers;
using RtfParseSpike.Templates;

namespace RtfParseTests.Parsers
{
    public class ChangesInCircsICLTests
    {
        private DirectoryInfo _testFixtureDirectory;
        private ChangesInCircsICL _parser;
        private DirectoryInfo _testResultsDirectory;

        [SetUp]
        public void Setup()
        {
            _parser = new ChangesInCircsICL();
            _testFixtureDirectory = new DirectoryInfo("./../../../TestFixtures/BlankBenefitsTemplate.cs/ExampleLettersRtf");
            _testResultsDirectory = new DirectoryInfo("./../../../TestFixtures/BlankBenefitsTemplate.cs/JSONTestResults");
        }

        [Test]
        public void ExecuteReturnsCorrectTitleAndClaimNumber()
        {
            _testFixtureDirectory.GetFiles().ToList().ForEach(fileInfo =>
            {
                var fileName = fileInfo.Name.Split(".").First();
                var testResultsPath = _testResultsDirectory + "/" + fileName + ".json";
                var expectedResults = JsonConvert.DeserializeObject<LetterTemplate>(
                    File.ReadAllText(testResultsPath));
            });
        }
    }
}