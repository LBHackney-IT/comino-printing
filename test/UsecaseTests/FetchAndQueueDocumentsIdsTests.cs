using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using UseCases;
using Usecases.Domain;
using Usecases.GatewayInterfaces;
using Usecases.Interfaces;

namespace UnitTests
{
    public class FetchAndQueueDocumentsIdsTests
    {
        private FetchAndQueueDocumentIds _subject;
        private Mock<IGetDocumentsIds> _getDocumentIdsMock;
        private Mock<IPushIdsToSqs> _pushIdsToSqs;
        private Mock<ISaveRecordsToLocalDatabase> _saveToLocalDatabase;
        private Fixture _fixture;
        private Mock<IDbLogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _getDocumentIdsMock = new Mock<IGetDocumentsIds>();
            _pushIdsToSqs = new Mock<IPushIdsToSqs>();
            _saveToLocalDatabase = new Mock<ISaveRecordsToLocalDatabase>();
            _logger = new Mock<IDbLogger>();
            _subject = new FetchAndQueueDocumentIds(_getDocumentIdsMock.Object, _pushIdsToSqs.Object, _saveToLocalDatabase.Object, _logger.Object);
        }

        [Test]
        public async Task ItGetsDocumentIds()
        {
            SetupSaveToLocalDatabase(SetupGetDocuments());
            await _subject.Execute();
            _getDocumentIdsMock.Verify();
        }

        [Test]
        public async Task ItPassesDocumentIdsToLocalDatabase()
        {
            var documents = SetupGetDocuments();
            SetupSaveToLocalDatabase(documents);
            await _subject.Execute();
            _saveToLocalDatabase.Verify();
        }

        [Test]
        public async Task ItAddsAMessageToTheLogWhenDocumentsAreStored()
        {
            var documents = SetupGetDocuments();
            var docsWithTimestamps = SetupSaveToLocalDatabase(documents);
            await _subject.Execute();
            docsWithTimestamps.ForEach(doc => _logger.Verify(x => x.LogMessage(doc.SavedAt, "Retrieved ID from Comino and stored")));
        }

        [Test]
        public async Task  ItPushesDocumentsToQueue()
        {
            var documents = SetupGetDocuments();
            var docsWithTimestamps = SetupSaveToLocalDatabase(documents);
            await _subject.Execute();

            var expectedTimestamps = docsWithTimestamps.Select(x => x.SavedAt).ToList();
            _pushIdsToSqs.Verify(x => x.Execute(expectedTimestamps), Times.Once);
        }

        [Test]
        public async Task ItAddsAMessageToTheLogWhenDocumentsArePushedToTheQueue()
        {
            var documents = SetupGetDocuments();
            var docsWithTimestamps = SetupSaveToLocalDatabase(documents);
            await _subject.Execute();
            docsWithTimestamps.ForEach(doc => _logger.Verify(x => x.LogMessage(doc.SavedAt, "Document added to SQS queue")));
        }

        [Test]
        public void IfSavingToSqsThrows_LogsError_ThenThrows()
        {
            var documents = SetupGetDocuments();
            var docsWithTimestamps = SetupSaveToLocalDatabase(documents);

            _pushIdsToSqs.Setup(x => x.Execute(It.IsAny<List<string>>())).Throws(new Exception("My custom exception"));

            var testRun = new AsyncTestDelegate(async () => await _subject.Execute());
            Assert.ThrowsAsync<Exception>(testRun);
            docsWithTimestamps.ForEach(doc => _logger.Verify(x => x.LogMessage(doc.SavedAt, "Failed adding to queue. Error message My custom exception")));
        }

        private List<DocumentDetails> SetupGetDocuments()
        {
            var documentDetails = _fixture.CreateMany<DocumentDetails>().ToList();
            _getDocumentIdsMock.Setup(x => x.Execute()).Returns(documentDetails).Verifiable();
            return documentDetails;
        }

        private List<DocumentDetails> SetupSaveToLocalDatabase(List<DocumentDetails> documents)
        {
            var docsWithTimestamp = _fixture.CreateMany<DocumentDetails>().ToList();
            _saveToLocalDatabase.Setup(x => x.Execute(documents)).ReturnsAsync(docsWithTimestamp).Verifiable();
            return docsWithTimestamp;
        }
    }
}