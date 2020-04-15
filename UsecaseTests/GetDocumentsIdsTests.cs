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
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _gatewayMock = new Mock<ICominoGateway>();
            _getDocumentsIds = new GetDocumentsIds(_gatewayMock.Object);
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

        private static DateTime CheckDateWithinASecond(DateTime startDate)
        {
            return It.Is<DateTime>(time => (time - startDate).TotalMilliseconds < 1000);
        }
    }
}
