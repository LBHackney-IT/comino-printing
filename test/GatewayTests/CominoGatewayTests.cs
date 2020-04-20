using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Gateways;
using Moq;
using Moq.Dapper;
using NUnit.Framework;
using Usecases.Domain;

namespace GatewayTests
{
    public class CominoGatewayTests
    {
        private CominoGateway _subject;
        private Mock<IDbConnection> _connection;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _connection = new Mock<IDbConnection>();
            _subject = new CominoGateway(_connection.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void GetDocumentsAfterStartDateSendsCorrectQueryToRepository()
        {
            var time = new DateTime(06, 12, 30, 00, 38, 54);
            const string expectedQuery = @"SELECT DocNo AS DocumentNumber,
StoreDate AS Date,
strDescription AS LetterType,
strUser AS UserName,
RefType AS DocumentType
FROM W2BatchPrint
JOIN CCDocument on DocNo = nDocNo
WHERE CCDocument.DocCategory = 'Benefits/Out-Going'
AND CCDocument.DirectionFg = 'O'
AND CCDocument.DocSource = 'O'
AND CCDocument.DocDate > 12/30/6 0:38:54
ORDER BY CCDocument.DocDate DESC;
";

            var stubbedResponseFromDb = _fixture.CreateMany<CominoGateway.W2BatchPrintRow>().ToList();
            var expectedResponse = MapDatabaseRowToDomain(stubbedResponseFromDb);

            SetupMockQueryToReturn(expectedQuery, stubbedResponseFromDb);

            var response = _subject.GetDocumentsAfterStartDate(time);

            response.Should().BeEquivalentTo(expectedResponse);
            _connection.Verify();
        }

        [Test]
        public void GetDocumentsAfterAnyStartDateSendsCorrectQueryToRepository()
        {
            var time = new DateTime(12, 11, 25,06, 13, 45);
            var expectedQuery =
@"SELECT DocNo AS DocumentNumber,
StoreDate AS Date,
strDescription AS LetterType,
strUser AS UserName,
RefType AS DocumentType
FROM W2BatchPrint
JOIN CCDocument on DocNo = nDocNo
WHERE CCDocument.DocCategory = 'Benefits/Out-Going'
AND CCDocument.DirectionFg = 'O'
AND CCDocument.DocSource = 'O'
AND CCDocument.DocDate > 11/25/12 6:13:45
ORDER BY CCDocument.DocDate DESC;
";
            var stubbedResponseFromDb = _fixture.CreateMany<CominoGateway.W2BatchPrintRow>().ToList();
            var expectedResponse = MapDatabaseRowToDomain(stubbedResponseFromDb);

            SetupMockQueryToReturn(expectedQuery, stubbedResponseFromDb);

            var response = _subject.GetDocumentsAfterStartDate(time);

            response.Should().BeEquivalentTo(expectedResponse);
            _connection.Verify();
        }

        private void SetupMockQueryToReturn(string expectedQuery, List<CominoGateway.W2BatchPrintRow> stubbedResponseFromDb)
        {
            _connection
                .SetupDapper(c => c.Query<CominoGateway.W2BatchPrintRow>(expectedQuery, null, null, true, null, null))
                .Returns(stubbedResponseFromDb)
                .Verifiable();
        }

        private static IEnumerable<DocumentDetails> MapDatabaseRowToDomain(List<CominoGateway.W2BatchPrintRow> stubbedResponseFromDb)
        {
            return stubbedResponseFromDb.Select(doc => new DocumentDetails
            {
                DocumentCreator = doc.UserName,
                DocumentId = doc.DocumentNumber,
                LetterType = doc.LetterType,
                DocumentType = doc.DocumentType,
            });
        }
    }
}