using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Model;
using AutoFixture;
using FluentAssertions;
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
        public void ExecuteCallsTheGatewayOnEachDocumentId()
        {
            var documentIds = _fixture.CreateMany<string>().ToList();
            
            _pushIdsToSqs.Execute(documentIds);

            foreach (var docId in documentIds)
            {
                _gatewayMock
                    .Verify(x => x.AddDocumentIdsToQueue(docId), Times.Once);
            }
        }
        
        [Test]
        public void ExecuteReturnsAListOfMessageResponses()
        {
            var documentIds = _fixture.CreateMany<string>().ToList();
                
            var expectedList = documentIds.Select(docId => new SendMessageResponse {MD5OfMessageBody = $"{docId}"}).ToList();

            foreach (var docId in documentIds)
            {
                _gatewayMock
                    .Setup(x => x.AddDocumentIdsToQueue(docId))
                    .Returns((string id) => new SendMessageResponse
                    {
                        MD5OfMessageBody = $"{id}"
                    });
            }
            
            var response = _pushIdsToSqs.Execute(documentIds);

            response.Should().BeEquivalentTo(expectedList);
        }
    }
}