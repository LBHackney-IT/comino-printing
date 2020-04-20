using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class SavePdfToS3Tests
    {
        private SavePdfToS3 _savePdfToS3;
        private Mock<IS3Gateway> _gatewayMock;

        [SetUp]
        public void Setup()
        {
            _gatewayMock = new Mock<IS3Gateway>();
            _savePdfToS3 = new SavePdfToS3(_gatewayMock.Object);
        }

        [Test]
        public void ExecuteCallsTheGatewayWithTheDocumentId()
        {
            var documentId = "123456";
            var filename = "[filename]";

            _savePdfToS3.Execute(documentId, filename);

            _gatewayMock.Verify(gateway => gateway.SavePdfDocument(documentId, filename), Times.Once());
        }
    }
}