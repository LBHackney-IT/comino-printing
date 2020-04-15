using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Moq;
using NUnit.Framework;
using UseCases;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class GetDocumentsIdsTests
    {
        private Mock<ICominoGateway> _gatewayMock;
        private GetDocumentsIds _getDocumentsIds;
        private Mock<ISqsGateway> _sqsGatewayMock;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _gatewayMock = new Mock<ICominoGateway>();
            _sqsGatewayMock = new Mock<ISqsGateway>();
            _getDocumentsIds = new GetDocumentsIds(_gatewayMock.Object, _sqsGatewayMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void ExecuteQueriesCominoForNewDocuments()
        {
            _getDocumentsIds.Execute();

            _gatewayMock
                .Verify(x => x.GetDocumentsAfterStartDate(It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public void ExecuteQueriesCominoForNewDocumentsWithCorrectTime()
        {
            var startDate = DateTime.Now.AddMinutes(-1);

            _getDocumentsIds.Execute();

            _gatewayMock
                .Verify(x => x.GetDocumentsAfterStartDate(CheckDateWithinASecond(startDate)), Times.Once);
        }

        [Test]
        public void ExecutePassesDocumentIdsFromGatewayToSqs()
        {
            var startDate = DateTime.Now.AddMinutes(-1);
            var documentsIds = _fixture.CreateMany<string>().ToList();
            _gatewayMock.Setup(x => x.GetDocumentsAfterStartDate(CheckDateWithinASecond(startDate))).Returns(documentsIds);

            _getDocumentsIds.Execute();

            _sqsGatewayMock.Verify(x => x.AddDocumentIdsToQueue(documentsIds), Times.Once);
        }

        private static DateTime CheckDateWithinASecond(DateTime startDate)
        {
            return It.Is<DateTime>(time => (time - startDate).TotalMilliseconds < 1000);
        }
    }
}