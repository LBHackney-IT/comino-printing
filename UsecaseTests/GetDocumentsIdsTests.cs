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
            
        [SetUp]
        public void Setup()
        {
            _gatewayMock = new Mock<ICominoGateway>();
            _sqsGatewayMock = new Mock<ISqsGateway>();
            _getDocumentsIds = new GetDocumentsIds(_gatewayMock.Object);
        }

        [Test]
        public void ExecuteQueriesCominoForNewDocuments()
        {
            _getDocumentsIds.Execute();
            var startDate = DateTime.Now.AddMinutes(-1);
            
            _gatewayMock
                .Verify(x => x.GetDocumentsAfterStartDate(It.IsAny<DateTime>()), Times.Once);
        }
        
        [Test]
        public void ExecuteQueriesCominoForNewDocumentsWithCorrectTime()
        {
            _getDocumentsIds.Execute();
            var startDate = DateTime.Now.AddMinutes(-1);
            
            _gatewayMock
                .Verify(x => x.GetDocumentsAfterStartDate(CheckDateWithinASecond(startDate)), Times.Once);
            
        }
        
        [Test]
        public void ExecutePassesDocumentIdsFromGatewayToSqs()
        {
            var startDate = DateTime.Now.AddMinutes(-1);
            var documentsIds = new List<string> { "AAAAAA" };
            
            _gatewayMock.Setup(x => x.GetDocumentsAfterStartDate(CheckDateWithinASecond(startDate))).Returns(documentsIds);
            _getDocumentsIds.Execute();
            
        }
        
        private static DateTime CheckDateWithinASecond(DateTime startDate)
        {
            return It.Is<DateTime>(time => (time - startDate).TotalMilliseconds < 1000);
        }
    }
}