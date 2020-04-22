using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using Usecases.Enums;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class GetDetailsOfDocumentForProcessingTests
    {
        private Mock<ILocalDatabaseGateway> _gateway;
        private GetDetailsOfDocumentForProcessing _subject;
        private IFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _gateway = new Mock<ILocalDatabaseGateway>();
            _subject = new GetDetailsOfDocumentForProcessing(_gateway.Object);
        }

        [Test]
        public async Task ItPassesANewStatusOfProcessingToTheGatewayWithTimestamp()
        {
            var timeStamp = _fixture.Create<string>();

            await _subject.Execute(timeStamp);

            _gateway.Verify(x => x.RetrieveDocumentAndSetStatusToProcessing(timeStamp), Times.Once);
        }

        [Test]
        public async Task ItReturnsTheDocumentDetailsRetrievedFromTheDetailsFromTheGateway()
        {
            var timeStamp = _fixture.Create<string>();
            var documentStub = _fixture.Create<DocumentDetails>();

            _gateway.Setup(x => x.RetrieveDocumentAndSetStatusToProcessing(timeStamp)).ReturnsAsync(documentStub);

            var response = await _subject.Execute(timeStamp);
            response.Should().BeEquivalentTo(documentStub);

        }
    }
}