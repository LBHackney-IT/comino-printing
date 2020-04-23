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
using UseCases;
using Usecases.Domain;
using Usecases.Enums;

namespace GatewayTests
{
    [Ignore("to fix")]
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
        public async Task GetAllRecordsReturnsAllDocumentRecords()
        {
            var savedDocumentOne = RandomDocumentDetails();
            await _dbGateway.SaveDocument(savedDocumentOne);
            
            var savedDocumentTwo = RandomDocumentDetails();
            await _dbGateway.SaveDocument(savedDocumentTwo);

            var expectedResponse = new List<DocumentDetails> {savedDocumentOne, savedDocumentTwo};
            
            var endId = new EndIdParameter();

            var response = await _dbGateway.GetAllRecords(endId);

            response.Should().BeEquivalentTo(expectedResponse);
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

            await _dbGateway.RetrieveDocumentAndSetStatusToProcessing(savedDocument.SavedAt);

            var savedDoc = await DatabaseClient.DocumentTable.GetItemAsync(savedDocument.SavedAt);
            savedDoc["Status"].ToString().Should().Be(LetterStatusEnum.Processing.ToString());
        }

        [Test]
        public async Task RetrieveDocumentAndSetStatusToProcessing_ReturnsTheDocument()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());

            var response = await _dbGateway.RetrieveDocumentAndSetStatusToProcessing(savedDocument.SavedAt);

            response.Should().BeEquivalentTo(savedDocument);
        }

        [Test]
        public async Task UpdateStatus_SetsNewStatusForADocument()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var newStatus = LetterStatusEnum.ProcessingError;

            await _dbGateway.UpdateStatus(savedDocument.SavedAt, newStatus);

            var savedDoc = await DatabaseClient.DocumentTable.GetItemAsync(savedDocument.SavedAt);
            savedDoc["Status"].ToString().Should().Be(newStatus.ToString());
        }

        [Test]
        public async Task LogMessage_WhenDoesntExist_CreatesLog()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var logMessage = "Something has happened";
            await _dbGateway.LogMessage(savedDocument.SavedAt, logMessage);
            var savedItems = await GetItemsFromDatabase();
            var log = GetLog(savedItems, savedDocument.SavedAt);

            log.Values.Select(s => s.ToString()).Should().Contain(logMessage);
        }

        [Test]
        public async Task LogMessage_WhenLogExists_AppendsMessageToLog()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var previousLog = new AttributeValue
            {
                M = new Dictionary<string, AttributeValue>
                {
                    {(GetCurrentTimestamp() - 1000).ToString(), new AttributeValue {S = "I was made"}},
                }
            };

            await UpdateLog(savedDocument, previousLog);

            var logMessage = "Something has happened";
            await _dbGateway.LogMessage(savedDocument.SavedAt, logMessage);

            var savedItems = await GetItemsFromDatabase();
            var log = GetLog(savedItems, savedDocument.SavedAt);

            log.Values.Select(s => s.ToString()).Should().Contain(logMessage);
        }

        [Test]
        public async Task GetLogForDocument_ReturnsAllLogEntries()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var currentTime = GetCurrentTimestamp();
            var logEntries = new AttributeValue
            {
                M = new Dictionary<string, AttributeValue>
                {
                    {(currentTime - 50).ToString(), new AttributeValue {S = "I was made"}},
                    {(currentTime - 30).ToString(), new AttributeValue {S = "then this happened"}},
                    {(currentTime - 10).ToString(), new AttributeValue {S = "then something else happened"}},
                    {currentTime.ToString(), new AttributeValue {S = "that was the end"}}
                }
            };

            await UpdateLog(savedDocument, logEntries);

            var expectedLog = new Dictionary<string, string>
            {
                {(currentTime - 50).ToString(), "I was made"},
                {(currentTime - 30).ToString(), "then this happened"},
                {(currentTime - 10).ToString(), "then something else happened"},
                {currentTime.ToString(), "that was the end"}
            };
            var receivedLog = _dbGateway.GetLogForDocument(savedDocument.SavedAt);
            receivedLog.Entries.Should().BeEquivalentTo(expectedLog);
        }

        private async Task UpdateLog(DocumentDetails savedDocument, AttributeValue logEntries)
        {
            var update = new UpdateItemRequest
            {
                TableName = DatabaseClient.DocumentTable.TableName,
                UpdateExpression = "SET #atr = :val",
                Key = new Dictionary<string, AttributeValue>
                    {{"InitialTimestamp", new AttributeValue {S = savedDocument.SavedAt}}},
                ExpressionAttributeNames = new Dictionary<string, string> {{"#atr", "Log"}},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {{":val", logEntries}}
            };
            await DatabaseClient.DatabaseClient.UpdateItemAsync(update);
        }

        private static Document GetLog(List<Document> savedItems, string savedAt)
        {
            return savedItems.First(i => i["InitialTimestamp"] == savedAt)["Log"].AsDocument();
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
