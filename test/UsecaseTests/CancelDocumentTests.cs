using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Usecases;
using Usecases.Domain;
using Usecases.Enums;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class CancelDocumentTests
    {
        private Mock<ILocalDatabaseGateway> _dbGatewayMock;
        private CancelDocument _subject;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _subject = new CancelDocument(_dbGatewayMock.Object);
            _fixture = new Fixture();
        }
        
        
        [Test]
        public async Task Execute_WillCallTheGatewayToUpdateTheStatusOfTheDocumentToCancelled()
        {
            var requestId = _fixture.Create<DocumentDetails>().Id;
        
            await _subject.Execute(requestId);
        
            _dbGatewayMock.Verify(x => x.UpdateStatus(requestId, LetterStatusEnum.Cancelled));
        }
    }
}