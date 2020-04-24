using System;
using AutoFixture;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using Usecases.Interfaces;

namespace UnitTests
{
    public class ConvertHtmlToPdfTests
    {
        private ConvertHtmlToPdf _convertHtmlToPdf;
        private Fixture _fixture;
        private Mock<IGetParser> _getParsers;
        private Mock<IParseHtmlToPdf> _parseHtmlToPdf;
        private Mock<ILetterParser> _mockLetterTypeParser;
        private string _convertHtmlToPdfEnvVar;

        [SetUp]
        public void Setup()
        {
            _convertHtmlToPdfEnvVar = Environment.GetEnvironmentVariable("CONVERT_HTML_TO_PDF");
            Environment.SetEnvironmentVariable("CONVERT_HTML_TO_PDF", "true");
            _fixture = new Fixture();
            _getParsers = new Mock<IGetParser>();
            _parseHtmlToPdf = new Mock<IParseHtmlToPdf>();
            _convertHtmlToPdf = new ConvertHtmlToPdf(_parseHtmlToPdf.Object, _getParsers.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("CONVERT_HTML_TO_PDF", _convertHtmlToPdfEnvVar);
        }

        [Test]
        public void ExecuteGetParserBasedOnDocType()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);

            _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);
            _getParsers.Verify();
        }

        [Test]
        public void ExecuteGetsHtmlFromCorrectParser()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);

            _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);

            _mockLetterTypeParser.Verify();
        }

        [Test]
        public void ExecuteWrapsHtmlFromParserInFurtherHtmlAndCss()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            var parsedHtml = SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);

            _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);
            _parseHtmlToPdf.Verify(x => x.Execute(
                It.Is<string>(html =>
                    AssertLetterDetailsIncludedIn(html, parsedHtml)), documentId, 5, 15, 5, 15), Times.Once);
        }

        [Test]
        public void ExecuteReturnsCallsThePdfConverterWithFileToSaveTo()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);

             _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);

             _parseHtmlToPdf.Verify(x => x.Execute(It.IsAny<string>(),documentId,  5, 15, 5, 15));
        }

        private LetterTemplate SetupLetterMockLetterTypeParser(string htmlDocument)
        {
            var parsedHtml = _fixture.Create<LetterTemplate>();

            var mockLetterTypeParser = new Mock<ILetterParser>();
            mockLetterTypeParser.Setup(x => x.Execute(htmlDocument)).Returns(parsedHtml).Verifiable();
            _mockLetterTypeParser = mockLetterTypeParser;
            return parsedHtml;
        }

        private void SetUpGetParser(string documentType)
        {
            _getParsers.Setup(x => x.ForType(documentType)).Returns(_mockLetterTypeParser.Object).Verifiable();
        }

        private static bool AssertLetterDetailsIncludedIn(string receivedHtml, LetterTemplate parsedHtml)
        {
            return receivedHtml.Contains(parsedHtml.Header)
                   && receivedHtml.Contains(parsedHtml.MainBody)
                   && receivedHtml.Contains(parsedHtml.TemplateSpecificCss);
        }
    }
}
