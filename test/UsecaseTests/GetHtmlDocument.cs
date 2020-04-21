using System.Threading.Tasks;
using AutoFixture;
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
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _gatewayMock = new Mock<IW2DocumentsGateway>();
            _getHtmlDocument = new GetHtmlDocument(_gatewayMock.Object);
        }

        [Test]
        public async Task ExecuteReturnsTheHtmlObtainedFromTheGateway()
        {
            var documentId = _fixture.Create<string>();
            var stubbedHtml = _fixture.Create<string>();
            _gatewayMock.Setup(gateway => gateway.GetHtmlDocument(documentId)).ReturnsAsync(stubbedHtml);

            var response = await _getHtmlDocument.Execute(documentId);
            response.Should().Be(stubbedHtml);
        }
    }
}
