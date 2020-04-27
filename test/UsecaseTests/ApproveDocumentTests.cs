using AutoFixture;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class ApproveDocumentTests
    {
        private Mock<ILocalDatabaseGateway> _dbGatewayMock;
        private ApproveDocument _subject;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _subject = new ApproveDocument(_dbGatewayMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void ExecuteWillCallTheGatewayToUpdateTheStatusOfTheDocumentAfterApproval()
        {
            var putRequestId = _fixture.Create<DocumentDetails>().Id;
            var requestedStatus = _fixture.Create<DocumentDetails>().Status;

            _subject.Execute(putRequestId);

            _dbGatewayMock.Verify(x => x.SetStatusToReadyForNotify(putRequestId));
        }
    }
}