using AutoFixture;
using Dapper;
using FluentAssertions;
using Gateways;
using Moq;
using Moq.Dapper;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnitTests;
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

            ConfigurationHelper.SetDocumentConfigEnvironmentVariable();
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
                WHERE CCDocument.DocCategory IN ('Benefits/Out-Going')
                AND CCDocument.DocDesc IN ('Income Verification Document')
                AND CCDocument.DirectionFg = 'O'
                AND CCDocument.DocSource = 'O'
                AND W2BatchPrint.StoreDate > '12/30/6 0:38:54'
                ORDER BY W2BatchPrint.StoreDate DESC;
                ";

            var stubbedResponseFromDb = _fixture.CreateMany<CominoGateway.W2BatchPrintRow>().ToList();
            var expectedResponse = MapDatabaseRowToDomain(stubbedResponseFromDb);

            SetupMockQueryToReturnDocuments(expectedQuery, stubbedResponseFromDb);

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
                WHERE CCDocument.DocCategory IN ('Benefits/Out-Going')
                AND CCDocument.DocDesc IN ('Income Verification Document')
                AND CCDocument.DirectionFg = 'O'
                AND CCDocument.DocSource = 'O'
                AND W2BatchPrint.StoreDate > 11/25/12 6:13:45
                ORDER BY W2BatchPrint.StoreDate DESC;
                ";
            var stubbedResponseFromDb = _fixture.CreateMany<CominoGateway.W2BatchPrintRow>().ToList();
            var expectedResponse = MapDatabaseRowToDomain(stubbedResponseFromDb);

            SetupMockQueryToReturnDocuments(expectedQuery, stubbedResponseFromDb);

            var response = _subject.GetDocumentsAfterStartDate(time);

            response.Should().BeEquivalentTo(expectedResponse);
            _connection.Verify();
        }

        [Test]
        public void GetDocumentSentStatus_IfDocumentIsNotInBatchPrint_ReturnsPrintedAndPrintedAt()
        {
            var cominoDocumentNumber = _fixture.Create<string>();
            var expectedLastPrintedAt = _fixture.Create<string>();

            var expectedQuery = $@"
SELECT TOP 1 LastPrinted, nDocNo
FROM CCDocument
LEFT JOIN W2BatchPrint ON nDocNo = DocNo
WHERE CCDocument.DocNo = '{cominoDocumentNumber}';
";

            SetupMockQueryToPrintedDetails(expectedQuery, null, expectedLastPrintedAt);

            var response = _subject.GetDocumentSentStatus(cominoDocumentNumber);
            response.Should().BeEquivalentTo(new CominoSentStatusCheck
            {
                Printed = true,
                PrintedAt = expectedLastPrintedAt
            });
        }

        [Test]
        public void GetDocumentSentStatus_IfDocumentLastPrintDateIsSet_ReturnsPrintedAndPrintedAt()
        {
            var cominoDocumentNumber = _fixture.Create<string>();
            var expectedLastPrintedAt = _fixture.Create<string>();
            var expectedQuery = $@"
SELECT TOP 1 LastPrinted, nDocNo
FROM CCDocument
LEFT JOIN W2BatchPrint ON nDocNo = DocNo
WHERE CCDocument.DocNo = '{cominoDocumentNumber}';
";
            SetupMockQueryToPrintedDetails(expectedQuery, cominoDocumentNumber, expectedLastPrintedAt);

            var response = _subject.GetDocumentSentStatus(cominoDocumentNumber);
            response.Should().BeEquivalentTo(new CominoSentStatusCheck
            {
                Printed = true,
                PrintedAt = expectedLastPrintedAt
            });
            _connection.Verify();
        }

        [Test]
        public void GetDocumentSentStatus_IfDocumentLastPrintDateIsNotSet_ReturnsNotPrinted()
        {
            var cominoDocumentNumber = _fixture.Create<string>();

            var expectedQuery = $@"
SELECT TOP 1 LastPrinted, nDocNo
FROM CCDocument
LEFT JOIN W2BatchPrint ON nDocNo = DocNo
WHERE CCDocument.DocNo = '{cominoDocumentNumber}';
";
            SetupMockQueryToPrintedDetails(expectedQuery, cominoDocumentNumber, null);

            var response = _subject.GetDocumentSentStatus(cominoDocumentNumber);
            response.Should().BeEquivalentTo(new CominoSentStatusCheck
            {
                Printed = false,
            });
            _connection.Verify();
        }

        private void SetupMockQueryToReturnDocuments(string expectedQuery, List<CominoGateway.W2BatchPrintRow> stubbedResponseFromDb)
        {
            _connection
                .SetupDapper(c => c.Query<CominoGateway.W2BatchPrintRow>(expectedQuery, null, null, true, null, null))
                .Returns(stubbedResponseFromDb)
                .Verifiable();
        }

        private void SetupMockQueryToPrintedDetails(string expectedQuery, string batchPrintNumber, string lastPrintedAt)
        {
            var response = new CominoGateway.PrintStatusRow
            {
                LastPrinted = lastPrintedAt,
                nDocNo = batchPrintNumber
            };
            _connection
                .SetupDapper(c => c.Query<CominoGateway.PrintStatusRow>(expectedQuery, null, null, true, null, null))
                .Returns(new List<CominoGateway.PrintStatusRow>{response})
                .Verifiable();
        }

        private static IEnumerable<DocumentDetails> MapDatabaseRowToDomain(List<CominoGateway.W2BatchPrintRow> stubbedResponseFromDb)
        {
            return stubbedResponseFromDb.Select(doc => new DocumentDetails
            {
                DocumentCreator = doc.UserName,
                CominoDocumentNumber = doc.DocumentNumber,
                LetterType = doc.LetterType,
                DocumentType = doc.DocumentType,
                Date = doc.Date.ToString("O"),
                Id = doc.Date.ToString("O"),
            });
        }
    }
}
