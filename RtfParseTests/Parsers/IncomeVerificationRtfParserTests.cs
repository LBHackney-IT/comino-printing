using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RtfParseSpike.Parsers;

namespace RtfParseTests
{
    public class IncomeVerificationRtfParserTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ExecuteReturnsNull()
        {
            var directory = new DirectoryInfo("./../../../ExampleLettersHtml");
            var fileInfo = directory.GetFiles().FirstOrDefault();

            var parser = new IncomeVerificationRtfParser();

            parser.Execute(fileInfo).Should().BeEquivalentTo(new IncomeVerificationTemplate
            {
                Title = "BENEFIT INCOME VERIFICATION DOCUMENT",
                ClaimNumber = "60065142"
            });
        }
    }

    public class IncomeVerificationTemplate
    {
        public string Title { get; set; }
        public string ClaimNumber { get; set; }
    }
}