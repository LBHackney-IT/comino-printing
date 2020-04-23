using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using comino_print_api.Responses;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using Usecases.Enums;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class UpdateDocumentStatusTests
    {
        private Mock<ILocalDatabaseGateway> _dbGatewayMock;
        private UpdateDocumentStatus _subject;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _subject = new UpdateDocumentStatus(_dbGatewayMock.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void ExecuteWillUpdateTheStatusOfTheDocument()
        {
            var savedRecord = _fixture.Create<DocumentDetails>();
            savedRecord.Status = LetterStatusEnum.Waiting;

            var newStatus = LetterStatusEnum.Processing;
            
            var expectedResponse = new DocumentResponse
            {
                Id = savedRecord.SavedAt,
                DocNo = savedRecord.DocumentId,
                Sender = savedRecord.DocumentCreator,
                Created = savedRecord.SavedAt,
                Status = newStatus.ToString(),
                LetterType = savedRecord.LetterType,
                DocumentType = savedRecord.DocumentType,
                Logs = savedRecord.Log.Select(x => new Dictionary<string, string>
                {
                    {"Date", x.Key},
                    {"Message", x.Value}

                }).ToList()
            };

            // _dbGatewayMock.Setup(x => x.UpdateStatus(savedRecord.SavedAt, newStatus));
            // _subject.Execute();
            //
            // _dbGatewayMock.Verify(x => x.);
            //
            // response.Documents.Should().BeEquivalentTo(expectedResponse);
        }
    }

    internal class UpdateDocumentStatus
    {
        public UpdateDocumentStatus(ILocalDatabaseGateway localDatabaseGateway)
        {
        }

        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}