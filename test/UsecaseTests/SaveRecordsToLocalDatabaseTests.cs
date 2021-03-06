using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using UseCases.GatewayInterfaces;

namespace UnitTests
{
    public class SaveRecordsToLocalDatabaseTests
    {
        private Mock<ILocalDatabaseGateway> _gatewayMock;
        private SaveRecordsToLocalDatabase _subject;
        private IFixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _gatewayMock = new Mock<ILocalDatabaseGateway>();
            _subject = new SaveRecordsToLocalDatabase(_gatewayMock.Object);
        }

        [Test]
        public async Task ItPassesAllDocumentsToGateway()
        {
            var documentsToSave = _fixture.Build<DocumentDetails>()
                .Without(doc => doc.Id).CreateMany().ToList();
            await _subject.Execute(documentsToSave);

            foreach (var document in documentsToSave)
            {
                _gatewayMock.Verify(x => x.SaveDocument(document), Times.Once);
            }
        }

        [Test]
        public async Task ItReturnsTheDocumentsDetailsWithPopulatedTimestamps()
        {
            var documentsToSave = _fixture.Build<DocumentDetails>()
                .Without(doc => doc.Id)
                .Without(doc => doc.Log)
                .Without(doc => doc.GovNotifyNotificationId)
                .CreateMany().ToList();

            var expectedDocuments = documentsToSave.Select(doc =>
            {
                _gatewayMock.Setup(x => x.SaveDocument(doc)).ReturnsAsync(true);
                return new DocumentDetails
                {
                    DocumentCreator = doc.DocumentCreator,
                    CominoDocumentNumber = doc.CominoDocumentNumber,
                    Id = doc.Date,
                    DocumentType = doc.DocumentType,
                    LetterType = doc.LetterType,
                    Status = doc.Status,
                    Date = doc.Date
                };
            }).ToList();

            var response = await _subject.Execute(documentsToSave);

            response.Should().BeEquivalentTo(expectedDocuments);
        }

        [Test]
        public async Task ItOnlyReturnsSavedDocumentsDetails()
        {
            var documents = _fixture.Build<DocumentDetails>()
                .Without(doc => doc.Id)
                .Without(doc => doc.Log)
                .Without(doc => doc.GovNotifyNotificationId)
                .CreateMany(2).ToList();
            var savedDocument = documents.First();
            var notSavedDocument = documents.Last();

            _gatewayMock.Setup(x => x.SaveDocument(savedDocument)).ReturnsAsync(true);
            _gatewayMock.Setup(x => x.SaveDocument(notSavedDocument)).ReturnsAsync(false);

            var response = await _subject.Execute(documents);
            response.Should().BeEquivalentTo(new List<DocumentDetails> {savedDocument});
        }
    }
}
