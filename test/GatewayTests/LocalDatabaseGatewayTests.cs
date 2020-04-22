using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using FluentAssertions;
using Gateways;
using NUnit.Framework;
using Usecases.Domain;
using Usecases.Enums;

namespace GatewayTests
{
    public class LocalDatabaseGatewayTests : DynamoDbTests
    {
        private Fixture _fixture;
        private LocalDatabaseGateway _dbGateway;

        [SetUp]
        public void SetUp()
        {
            _dbGateway = new LocalDatabaseGateway(DatabaseClient);
            _fixture = new Fixture();
        }

        [Test]
        public async Task AddRecordForDocumentId_SavesARecordToTheDatabase_WithAllDocumentDetails()
        {
            var newDocument = RandomDocumentDetails();
            await _dbGateway.SaveDocument(newDocument);

            var response = await GetItemsFromDatabase();

            response
                .Where(doc => doc["DocumentId"] == newDocument.DocumentId)
                .Where(doc => doc["LetterType"] == newDocument.LetterType)
                .Where(doc => doc["DocumentType"] == newDocument.DocumentType)
                .Count(doc => doc["DocumentCreatorUserName"] == newDocument.DocumentCreator)
                .Should().Be(1);
        }

        [Test]
        public async Task AddRecordForDocumentId_SavesARecordToTheDatabase_WithCorrectTimestamp()
        {
            var newDocument = RandomDocumentDetails();
            await _dbGateway.SaveDocument(newDocument);
            var expectedTimestamp = Convert.ToInt32((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);

            var response = await GetItemsFromDatabase();
            var savedTimestamp = GetTimestampForDocumentId(response, newDocument);
            savedTimestamp.Should().BeCloseTo(expectedTimestamp, 1);
        }

        [Test]
        public async Task AddRecordForDocumentId_ReturnsTheTimestampCreated()
        {
            var newDocument = RandomDocumentDetails();
            var response = await _dbGateway.SaveDocument(newDocument);
            var currentTimestamp = GetCurrentTimestamp();

            Convert.ToInt32(response).Should().BeCloseTo(currentTimestamp, 1);
        }

        [Test]
        public async Task AddRecordForDocumentId_IfTwoRecordsAddedAtTheSameTime_NeitherIsOverwritten()
        {
            var currentTimestamp = GetCurrentTimestamp();

            var document1 = RandomDocumentDetails();
            await AddDocumentToDatabase(document1, currentTimestamp);

            var document2 = RandomDocumentDetails();
            await _dbGateway.SaveDocument(document2);

            var savedItems = await GetItemsFromDatabase();

            savedItems.Count(doc => doc["DocumentId"] == document1.DocumentId).Should().Be(1);
            savedItems.Count(doc => doc["DocumentId"] == document2.DocumentId).Should().Be(1);
        }

        [Test]
        public async Task AddRecordForDocumentId_IfTwoRecordsAddedAtTheSameTime_ReturnsTimestampFromRetry()
        {
            var currentTimestamp = GetCurrentTimestamp();

            var document1 = RandomDocumentDetails();
            await AddDocumentToDatabase(document1, currentTimestamp);

            var document2 = RandomDocumentDetails();
            var response = await _dbGateway.SaveDocument(document2);

            Convert.ToInt32(response).Should().BeGreaterThan(currentTimestamp);
        }

        [Test]
        public async Task GetRecordByTimeStamp_RetrievesRecordFromDb()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());

            var response = await _dbGateway.GetRecordByTimeStamp(savedDocument.SavedAt);

            response.Should().BeEquivalentTo(savedDocument);
        }

        [Test]
        public async Task RetrieveDocumentAndSetStatusToProcessing_UpdatesADocumentsStatus()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var newStatus = LetterStatusEnum.Processing;

            await _dbGateway.RetrieveDocumentAndSetStatusToProcessing(savedDocument.SavedAt, newStatus);

            var savedDoc = await DatabaseClient.DocumentTable.GetItemAsync(savedDocument.SavedAt);
            savedDoc["Status"].ToString().Should().Be(newStatus.ToString());
        }

        [Test]
        public async Task RetrieveDocumentAndSetStatusToProcessing_ReturnsTheDocument()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var newStatus = LetterStatusEnum.Processing;

            var response = await _dbGateway.RetrieveDocumentAndSetStatusToProcessing(savedDocument.SavedAt, newStatus);

            response.Should().BeEquivalentTo(savedDocument);
        }

        [Test]
        public async Task LogMessage_AppendsMessageToLog()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var logMessage = "Something has happened";
            await _dbGateway.LogMessage(savedDocument.SavedAt, logMessage);
            var savedItems = await GetItemsFromDatabase();

            GetLastLogEntryMessage(savedItems, savedDocument.SavedAt).Should().BeEquivalentTo(logMessage);

            var timestampOfLogMessage = Convert.ToInt32(GetLastLogEntryTimestamp(savedItems, savedDocument.SavedAt));
            timestampOfLogMessage.Should().BeCloseTo(GetCurrentTimestamp(), 1);
        }

        private static string GetLastLogEntryMessage(List<Document> savedItems, string savedAt)
        {
            var savedLog = savedItems.First(i => i["InitialTimestamp"] == savedAt)["Log"];

            return savedLog.AsListOfDynamoDBEntry().First().AsDocument().First().Value;
        }

        private static string GetLastLogEntryTimestamp(List<Document> savedItems, string savedAt)
        {
            var savedLog = savedItems.First(i => i["InitialTimestamp"] == savedAt)["Log"];

            return savedLog.AsListOfDynamoDBEntry().First().AsDocument().First().Key;
        }

        private static int GetCurrentTimestamp()
        {
            return Convert.ToInt32((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
        }

        private DocumentDetails RandomDocumentDetails()
        {
            return new DocumentDetails
            {
                DocumentId = _fixture.Create<string>(),
                DocumentCreator = _fixture.Create<string>(),
                LetterType = _fixture.Create<string>(),
                DocumentType = _fixture.Create<string>(),
            };
        }

        private async Task<DocumentDetails> AddDocumentToDatabase(DocumentDetails document, int? currentTimestamp = null)
        {
            var timestamp = currentTimestamp ?? GetCurrentTimestamp();
            var documentItem = new Document
            {
                ["DocumentId"] = document.DocumentId,
                ["DocumentCreatorUserName"] = document.DocumentCreator,
                ["InitialTimestamp"] = timestamp.ToString(),
                ["LetterType"] = document.LetterType,
                ["DocumentType"] = document.DocumentType,
                ["Log"] = new DynamoDBList(),
                ["Status"] = LetterStatusEnum.Waiting.ToString()
            };
            await DatabaseClient.DocumentTable.PutItemAsync(documentItem);

            document.SavedAt = timestamp.ToString();
            return document;
        }

        private static int GetTimestampForDocumentId(List<Document> savedItems, DocumentDetails document)
        {
            return (int) savedItems.First(doc => doc["DocumentId"] == document.DocumentId)["InitialTimestamp"];
        }

        private async Task<List<Document>> GetItemsFromDatabase()
        {
            var scanFilter = new ScanFilter();

            var search = DatabaseClient.DocumentTable.Scan(scanFilter);
            return await search.GetRemainingAsync();
        }
    }
}
