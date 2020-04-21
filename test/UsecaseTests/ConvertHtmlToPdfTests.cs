using System;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.UseCaseInterfaces;

namespace UnitTests
{
    public class ConvertHtmlToPdfTests
    {
        private ConvertHtmlToPdf _convertHtmlToPdf;
        private Fixture _fixture;
        private Mock<IGetParser> _getParsers;
        private Mock<IParseHtmlToPdf> _parseHtmlToPdf;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _getParsers = new Mock<IGetParser>();
            _parseHtmlToPdf = new Mock<IParseHtmlToPdf>();
            _convertHtmlToPdf = new ConvertHtmlToPdf(_parseHtmlToPdf.Object, _getParsers.Object);
        }

        [Test]
        public void ExecuteCalledWithHtmlDocumentReturnsAPdf()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = "<p>html document</p>";
            var expected = "[pdf document - filepath?]";
            var received = _convertHtmlToPdf.Execute(htmlDocument, documentType);

            received.Should().BeEquivalentTo(expected);
        }
    }
}
