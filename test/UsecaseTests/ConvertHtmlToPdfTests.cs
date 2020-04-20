using System;
using FluentAssertions;
using NUnit.Framework;
using UseCases;

namespace UnitTests
{
    public class ConvertHtmlToPdfTests
    {
        private ConvertHtmlToPdf _convertHtmlToPdf;

        [SetUp]
        public void Setup()
        {
            _convertHtmlToPdf = new ConvertHtmlToPdf();
        }

        [Test]
        public void ExecuteCalledWithHtmlDocumentReturnsAPdf()
        {
            var htmlDocument = "<p>html document</p>";
            var expected = "[pdf document - filepath?]";
            var received = _convertHtmlToPdf.Execute(htmlDocument);

            received.Should().BeEquivalentTo(expected);
        }
    }
}
