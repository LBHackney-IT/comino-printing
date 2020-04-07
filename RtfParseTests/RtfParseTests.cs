using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RtfParseSpike;

namespace RtfParseTests
{
    public class RtfParseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ExecuteReturnsNull()
        {
            var file = File.Open("./../../../ExampleLettersHtml/40712646.html", FileMode.Open);

            var spike = new RtfParse(file);
            spike.Execute().Should().BeEquivalentTo(new IncomeVerificationTemplate
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