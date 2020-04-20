using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
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
        public void ExecuteQueriesAndReturnsCominoForNewDocumentsWithCorrectTime()
        {
            var startDate = DateTime.Now.AddMinutes(-1);
            var documentIds = _fixture.CreateMany<DocumentDetails>().ToList();
            _gatewayMock
                .Setup(x => x.GetDocumentsAfterStartDate(CheckDateWithinASecond(startDate)))
                .Returns(documentIds)
                .Verifiable();
            _getDocumentsIds.Execute().Should().BeEquivalentTo(documentIds);

            _gatewayMock.Verify();
        }

        private static DateTime CheckDateWithinASecond(DateTime startDate)
        {
            return It.Is<DateTime>(time => (time - startDate).TotalMilliseconds < 1000);
        }
    }
}
