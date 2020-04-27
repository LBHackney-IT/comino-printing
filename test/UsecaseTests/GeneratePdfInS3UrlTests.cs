using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class GeneratePdfInS3UrlTests
    {
        private Mock<IS3Gateway> _gatewayMock;
        private GeneratePdfInS3Url _generatePdfInS3Url;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _gatewayMock = new Mock<IS3Gateway>();
            _generatePdfInS3Url = new GeneratePdfInS3Url(_gatewayMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void Execute_CallsGeneratePdfUrlOnTheS3Gateway()
        {
            var docId = _fixture.Create<string>();
            _generatePdfInS3Url.Execute(docId);
            
            _gatewayMock.Verify(x => x.GeneratePdfUrl(docId));
        }
    }
}