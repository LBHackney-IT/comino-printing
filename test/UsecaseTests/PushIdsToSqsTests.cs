using System;
using System.Linq;
using AutoFixture;
using Moq;
using NUnit.Framework;
using UseCases;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class PushIdsToSqsTests
    {
        private Mock<ISqsGateway> _gatewayMock;
        private  PushIdsToSqs _pushIdsToSqs;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _gatewayMock = new Mock<ISqsGateway>();
            _pushIdsToSqs = new PushIdsToSqs(_gatewayMock.Object);
            _fixture = new Fixture();
        }
        
        [Test]
        public void ExecutePushesDocumentIdsToSqs()
        {
            var documentIds = _fixture.CreateMany<string>().ToList();
            
            _pushIdsToSqs.Execute(documentIds);
            _gatewayMock
                .Verify(x => x.AddDocumentIdsToQueue(documentIds), Times.Once);
        }
    }
}