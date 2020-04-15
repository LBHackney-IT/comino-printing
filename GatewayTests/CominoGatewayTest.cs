using System;
using Gateways;
using Moq;
using NUnit.Framework;

namespace GatewayTests
{
    public class Tests
    {
        private Mock<IDatabaseRepository> _dbMock;
        private CominoGateway _subject;

        [SetUp]
        public void Setup()
        {
            _dbMock = new Mock<IDatabaseRepository>();
            _subject = new CominoGateway(_dbMock.Object);
        }

        [Test]
        public void GetDocumentsAfterStartDateSendsCorrectQueryToRepository()
        {
            var time = new DateTime(06, 12, 30, 00, 38, 54);
            _subject.GetDocumentsAfterStartDate(time);
            var expectedQuery =
@"SELECT DocNo FROM CCDocument
WHERE DocCategory = 'Benefits/Out-Going'
AND DirectionFg = 'O'
AND DocSource = 'O'
AND DocDate > 12/30/6 0:38:54
ORDER BY DocDate DESC;
";
            _dbMock.Verify(x => x.QueryBatchPrint(expectedQuery), Times.Once);
        }

        [Test]
        public void GetDocumentsAfterAnyStartDateSendsCorrectQueryToRepository()
        {
            var time = new DateTime(12, 11, 25,06, 13, 45);
            _subject.GetDocumentsAfterStartDate(time);
            var expectedQuery =
                @"SELECT DocNo FROM CCDocument
WHERE DocCategory = 'Benefits/Out-Going'
AND DirectionFg = 'O'
AND DocSource = 'O'
AND DocDate > 11/25/12 6:13:45
ORDER BY DocDate DESC;
";
            _dbMock.Verify(x => x.QueryBatchPrint(expectedQuery), Times.Once);
        }
    }
}