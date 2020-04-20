using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class GetHtmlDocumentTests
    {
        private GetHtmlDocument _getHtmlDocument;
        private Mock<IW2DocumentsGateway> _gatewayMock;

        [SetUp]
        public void Setup()
        {
            _gatewayMock = new Mock<IW2DocumentsGateway>();
            _getHtmlDocument = new GetHtmlDocument(_gatewayMock.Object);
        }

        [Test]
        public void ExecuteCallsTheGatewayWithTheDocumentId()
        {
            var documentId = "123456";

            _getHtmlDocument.Execute(documentId);

            _gatewayMock.Verify(gateway => gateway.GetHtmlDocument(documentId), Times.Once());
        }
    }
}
