using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.SQSEvents;
using AutoFixture;
using AwsDotnetCsharp.UsecaseInterfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class ProcessEventsTests
    {
        private ProcessEvents _processEvents;
        private Mock<IGetHtmlDocument> _mockGetHtmlDocument;
        private IFixture _fixture;
        private Mock<IConvertHtmlToPdf> _mockPdfParser;
        private Mock<ISavePdfToS3> _sendToS3;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _mockGetHtmlDocument = new Mock<IGetHtmlDocument>();
            _mockPdfParser = new Mock<IConvertHtmlToPdf>();
            _sendToS3 = new Mock<ISavePdfToS3>();
            _processEvents = new ProcessEvents(_mockGetHtmlDocument.Object, _mockPdfParser.Object, _sendToS3.Object);
        }

        [Test]
        public void ExecuteReceivesEventAndGetRelatedHtmlDocument()
        {
            var documentId = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(documentId);

            _processEvents.Execute(sqsEventMock);
            _mockGetHtmlDocument.Verify(x => x.Execute(documentId), Times.Once);
        }

        [Test]
        public void ParsersRetrievedHtmlToAPdf()
        {
            var documentId = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(documentId);
            var stubbedReturnHtml = _fixture.Create<string>();
            _mockGetHtmlDocument.Setup(x => x.Execute(documentId)).Returns(stubbedReturnHtml);

            _processEvents.Execute(sqsEventMock);

            _mockPdfParser.Verify(x => x.Execute(stubbedReturnHtml), Times.Once);
        }

        [Test]
        public void SavesTheConvertedPdfToS3()
        {
            var documentId = _fixture.Create<string>();
            var sqsEventMock = CreateSqsEventForDocumentId(documentId);
            //not sure what type this is yet - maybe a byte array???
            var stubbedPdf = _fixture.Create<string>();

            _mockGetHtmlDocument.Setup(x => x.Execute(documentId)).Returns(_fixture.Create<string>());
            _mockPdfParser.Setup(x => x.Execute(It.IsAny<string>())).Returns(stubbedPdf);

            _processEvents.Execute(sqsEventMock);

            _sendToS3.Verify(x => x.Execute(documentId, stubbedPdf), Times.Once);
        }

        [Test]
        public void SomethingAboutSavingStatusToLocalDatabase()
        {
            
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
    }
}
