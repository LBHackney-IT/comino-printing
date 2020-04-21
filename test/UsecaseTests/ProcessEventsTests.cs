using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using AutoFixture;
using AwsDotnetCsharp.UsecaseInterfaces;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using Usecases.UseCaseInterfaces;

namespace UnitTests
{
    public class ProcessEventsTests
    {
        private ProcessEvents _processEvents;
        private Mock<IGetHtmlDocument> _mockGetHtmlDocument;
        private IFixture _fixture;
        private Mock<IConvertHtmlToPdf> _mockPdfParser;
        private Mock<ISavePdfToS3> _sendToS3;
        private Mock<IGetDetailsOfDocumentForProcessing> _mockGetDocDetails;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockGetHtmlDocument = new Mock<IGetHtmlDocument>();
            _mockPdfParser = new Mock<IConvertHtmlToPdf>();
            _sendToS3 = new Mock<ISavePdfToS3>();
            _mockGetDocDetails = new Mock<IGetDetailsOfDocumentForProcessing>();
            _processEvents = new ProcessEvents(_mockGetHtmlDocument.Object, _mockPdfParser.Object, _sendToS3.Object, _mockGetDocDetails.Object);
        }

        [Test]
        public async Task ExecuteReceivesEventAndGetsDocumentDetails()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            SetupGetDocumentDetails(timestamp);

            await _processEvents.Execute(sqsEventMock);
            _mockGetDocDetails.Verify();
        }

        [Test]
        public async Task ExecuteUsesDocumentNumberToGetRelatedHtmlDocument()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            var documentDetails = SetupGetDocumentDetails(timestamp);

            await _processEvents.Execute(sqsEventMock);
            _mockGetHtmlDocument.Verify(x => x.Execute(documentDetails.DocumentId), Times.Once);
        }


        [Test]
        public async Task  ParsesRetrievedHtmlToAPdf_AsCorrectDocumentType()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            var document = SetupGetDocumentDetails(timestamp);

            var stubbedReturnHtml = _fixture.Create<string>();
            _mockGetHtmlDocument.Setup(x => x.Execute(document.DocumentId)).ReturnsAsync(stubbedReturnHtml);

            await _processEvents.Execute(sqsEventMock);

            _mockPdfParser.Verify(x => x.Execute(stubbedReturnHtml, document.LetterType), Times.Once);
        }

        [Ignore("TO DO - what will be sent to S3")]
        [Test]
        public async Task  SavesTheConvertedPdfToS3()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            var documentId = SetupGetDocumentDetails(timestamp).DocumentId;

            var stubbedPdf = _fixture.CreateMany<byte>().ToArray();

            _mockGetHtmlDocument.Setup(x => x.Execute(documentId)).ReturnsAsync(_fixture.Create<string>());
            _mockPdfParser.Setup(x => x.Execute(It.IsAny<string>(),It.IsAny<string>())).Returns(stubbedPdf);

            await _processEvents.Execute(sqsEventMock);

            _sendToS3.Verify(x => x.Execute(documentId, stubbedPdf), Times.Once);
        }

        private static SQSEvent CreateSqsEventForDocumentId(string documentId)
        {
            var sqsMessageMock = new Mock<SQSEvent.SQSMessage>();
            sqsMessageMock.Object.Body = documentId;
            sqsMessageMock.Object.EventSourceArn = "arn:aws:sqs:eu-west-2:123456789012:DefaultQueue";

            var sqsEventMock = new Mock<SQSEvent>();
            sqsEventMock.Object.Records = new List<SQSEvent.SQSMessage>() {sqsMessageMock.Object};
            return sqsEventMock.Object;
        }

        private DocumentDetails SetupGetDocumentDetails(string timestamp)
        {
            var document = _fixture.Create<DocumentDetails>();
            _mockGetDocDetails.Setup(x => x.Execute(timestamp)).ReturnsAsync(document).Verifiable();
            return document;
        }
    }
}
