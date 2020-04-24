using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using comino_print_api.Responses;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class GetAllDocumentsTests
    {
        private Mock<ILocalDatabaseGateway> _dbGatewayMock;
        private GetAllDocuments _subject;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _subject = new GetAllDocuments(_dbGatewayMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void ExecuteReturnsAllRecordsSubjectToLimitAndCursorFromDatabaseGateway()
        {
            var savedRecord = _fixture.CreateMany<DocumentDetails>().ToList();
            var limit = _fixture.Create<int>();
            var cursor = _fixture.Create<string>();

            var expectedResponse = savedRecord.Select(record => new DocumentResponse
            {
                Id = record.SavedAt,
                DocNo = record.DocumentId,
                Sender = record.DocumentCreator,
                Created = record.SavedAt,
                Status = record.Status.ToString(),
                LetterType = record.LetterType,
                DocumentType = record.DocumentType,
                Logs = record.Log.Select(x => new Dictionary<string, string>
                {
                    {"date", x.Key},
                    {"message", x.Value}
                }).ToList()
            });

            _dbGatewayMock.Setup(x => x.GetAllRecords(limit, cursor)).ReturnsAsync(savedRecord);

            var response = _subject.Execute(limit.ToString(), cursor).Result;

            response.Documents.Should().BeEquivalentTo(expectedResponse);
        }
    }
}