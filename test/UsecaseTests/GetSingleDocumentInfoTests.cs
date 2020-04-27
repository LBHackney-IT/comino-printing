using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using comino_print_api.Responses;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Usecases;
using UseCases;
using Usecases.Domain;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class GetSingleDocumentInfoTests
    {
        private Mock<ILocalDatabaseGateway> _dbGatewayMock;
        private GetSingleDocumentInfo _subject;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _dbGatewayMock = new Mock<ILocalDatabaseGateway>();
            _subject = new GetSingleDocumentInfo(_dbGatewayMock.Object);
            _fixture = new Fixture();
        }
        
        [Test]
        public void Execute_ReturnsInformationAboutASingleRecordFromDatabaseGateway()
        {
            var savedRecord = _fixture.Create<DocumentDetails>();
            var id = savedRecord.Id;
            
            var expectedResponse = new DocumentResponse
            {
                Id = id,
                DocNo = savedRecord.CominoDocumentNumber,
                Sender = savedRecord.DocumentCreator,
                Created = savedRecord.Id,
                Status = savedRecord.Status.ToString(),
                LetterType = savedRecord.LetterType,
                DocumentType = savedRecord.DocumentType,
                Logs = savedRecord.Log.Select(x => new Dictionary<string, string>
                {
                    {"date", x.Key},
                    {"message", x.Value}
                }).ToList()
            };
            
            
            _dbGatewayMock.Setup(x => x.GetRecordByTimeStamp(id)).ReturnsAsync(savedRecord);
            
            var response = _subject.Execute(id).Result;
            
            response.Document.Should().BeEquivalentTo(expectedResponse);
        }
    }
}