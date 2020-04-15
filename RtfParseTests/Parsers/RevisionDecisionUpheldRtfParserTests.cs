using System;
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
    public class RevisionDecisionUpheldRtfParserTests
    {
        private DirectoryInfo _testFixtureDirectory;
        private RevisionDecisionUpheldRtfParser _parser;
        private DirectoryInfo _testResultsDirectory;

        [SetUp]
        public void Setup()
        {
            _parser = new RevisionDecisionUpheldRtfParser();
            _testFixtureDirectory = new DirectoryInfo("./../../../TestFixtures/RevisionDecisionUpheld/ExampleLettersHtml");
            _testResultsDirectory = new DirectoryInfo("./../../../JSONTestResults/RevisionDecisionUpheld");
        }

        [Test]
        public void ExecuteReturnsCorrectContent()
        {
            _testFixtureDirectory.GetFiles().ToList().ForEach(fileInfo =>
            {
                var fileName = fileInfo.Name.Split(".").First();
                var testResultsPath = _testResultsDirectory + "/" + fileName + ".json";
                var expectedResults = JsonConvert.DeserializeObject<RevisionDecisionUpheldTemplate>(
                    File.ReadAllText(testResultsPath)
                );

                _parser.Execute(fileInfo).HeaderTableRows.Should().BeEquivalentTo(expectedResults.HeaderTableRows);
                _parser.Execute(fileInfo).Content.Should().BeEquivalentTo(expectedResults.Content);
            });
        }
    }
}