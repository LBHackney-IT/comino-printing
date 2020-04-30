using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
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

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _getParsers = new Mock<IGetParser>();
            _parseHtmlToPdf = new Mock<IParseHtmlToPdf>();
            _convertHtmlToPdf = new ConvertHtmlToPdf(_parseHtmlToPdf.Object, _getParsers.Object);
        }

        [Test]
        public async Task ExecuteGetParserBasedOnDocType()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);

            await _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);
            _getParsers.Verify();
        }

        [Test]
        public async Task ExecuteGetsHtmlFromCorrectParser()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);

            await _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);

            _mockLetterTypeParser.Verify();
        }

        [Test]
        public async Task ExecuteWrapsHtmlFromParserInFurtherHtmlAndCss()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            var parsedHtml = SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);

            await _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);
            _parseHtmlToPdf.Verify(x => x.Convert(
                It.Is<string>(html =>
                    AssertLetterDetailsIncludedIn(html, parsedHtml)), documentId), Times.Once);
        }

        [Test]
        public async Task ExecuteCallsThePdfConverterWithHtml()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);
            SetupPdfConverter(documentId);

            await _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);
            _parseHtmlToPdf.Verify();
        }

        [Test]
        public async Task ExecuteSavesTheReturnedPdfToTmpFolder()
        {
            var documentType = _fixture.Create<string>();
            var htmlDocument = _fixture.Create<string>();
            var documentId = _fixture.Create<string>();

            SetupLetterMockLetterTypeParser(htmlDocument);
            SetUpGetParser(documentType);
            var expectedPdf = SetupPdfConverter(documentId);

            await _convertHtmlToPdf.Execute(htmlDocument, documentType, documentId);

            File.Exists($"/tmp/{documentId}.pdf").Should().BeTrue();
            var savedBytes = await File.ReadAllBytesAsync($"/tmp/{documentId}.pdf");
            savedBytes.Should().BeEquivalentTo(expectedPdf);
        }

        private byte[] SetupPdfConverter(string documentId)
        {
            var pdfBytes = _fixture.Create<byte[]>();
            _parseHtmlToPdf.Setup(x => x.Convert(It.IsAny<string>(), documentId)).ReturnsAsync(pdfBytes).Verifiable();
            return pdfBytes;
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
            return receivedHtml.Contains(parsedHtml.RightSideOfHeader)
                   && parsedHtml.AddressLines.TrueForAll(receivedHtml.Contains)
                   && receivedHtml.Contains(parsedHtml.MainBody)
                   && receivedHtml.Contains(parsedHtml.TemplateSpecificCss);
        }
    }
}
