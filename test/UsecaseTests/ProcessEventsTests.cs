using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using AutoFixture;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using Usecases.GatewayInterfaces;
using UseCases.GatewayInterfaces;
using Usecases.UseCaseInterfaces;

namespace UnitTests
{
    public class ProcessEventsTests
    {
        private ProcessEvents _processEvents;
        private Mock<IGetHtmlDocument> _mockGetHtmlDocument;
        private IFixture _fixture;
        private Mock<IConvertHtmlToPdf> _mockPdfParser;
        private Mock<IS3Gateway> _sendToS3;
        private Mock<IGetDetailsOfDocumentForProcessing> _mockGetDocDetails;
        private Mock<IDbLogger> _logger;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockGetHtmlDocument = new Mock<IGetHtmlDocument>();
            _mockPdfParser = new Mock<IConvertHtmlToPdf>();
            _sendToS3 = new Mock<IS3Gateway>();
            _mockGetDocDetails = new Mock<IGetDetailsOfDocumentForProcessing>();
            _logger = new Mock<IDbLogger>();
            _processEvents = new ProcessEvents(_mockGetHtmlDocument.Object, _mockPdfParser.Object, _sendToS3.Object,
                _mockGetDocDetails.Object, _logger.Object);
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
        public async Task ExecuteLogsPickingUpDocumentFromTheQueue()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            SetupGetDocumentDetails(timestamp);

            await _processEvents.Execute(sqsEventMock);
            _logger.Verify(l => l.LogMessage(timestamp, "Picked up document from queue - Processing"));
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
        public async Task ExecuteLogsThatHtmlDocumentHasBeenRetrieved()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            SetupGetDocumentDetails(timestamp);

            await _processEvents.Execute(sqsEventMock);
            _logger.Verify(l => l.LogMessage(timestamp, "Retrieved Html from Documents API"));
        }


        [Test]
        public async Task ParsesRetrievedHtmlToAPdf_AsCorrectDocumentType()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            var document = SetupGetDocumentDetails(timestamp);

            var stubbedReturnHtml = _fixture.Create<string>();
            _mockGetHtmlDocument.Setup(x => x.Execute(document.DocumentId)).ReturnsAsync(stubbedReturnHtml);

            await _processEvents.Execute(sqsEventMock);

            _mockPdfParser.Verify(x => x.Execute(stubbedReturnHtml, document.LetterType, document.DocumentId), Times.Once);
        }

        [Test]
        public async Task IfThePdfParserIsSuccessfulLogThis()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            var documentId = SetupGetDocumentDetails(timestamp).DocumentId;

            _mockGetHtmlDocument.Setup(x => x.Execute(documentId)).ReturnsAsync(_fixture.Create<string>());

            await _processEvents.Execute(sqsEventMock);

            _logger.Verify(l => l.LogMessage(timestamp, "Converted To Pdf"));
        }

        [Test]
        public async Task IfThePdfParserIsSuccessfulSaveToS3()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            var documentId = SetupGetDocumentDetails(timestamp).DocumentId;

            _mockGetHtmlDocument.Setup(x => x.Execute(documentId)).ReturnsAsync(_fixture.Create<string>());

            await _processEvents.Execute(sqsEventMock);

            _sendToS3.Verify(x => x.SavePdfDocument(documentId), Times.Once);
        }

        [Test]
        public async Task IfStoringInS3IsSuccessfulLogsThis()
        {
            var timestamp = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(timestamp);
            var documentId = SetupGetDocumentDetails(timestamp).DocumentId;

            _mockGetHtmlDocument.Setup(x => x.Execute(documentId)).ReturnsAsync(_fixture.Create<string>());

            await _processEvents.Execute(sqsEventMock);

            _logger.Verify(l => l.LogMessage(timestamp, "Stored in S3 - Ready for approval"));
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
