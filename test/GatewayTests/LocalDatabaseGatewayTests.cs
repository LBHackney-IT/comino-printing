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
                .Where(doc => doc["CominoDocumentNumber"] == newDocument.CominoDocumentNumber)
                .Where(doc => doc["LetterType"] == newDocument.LetterType)
                .Where(doc => doc["DocumentType"] == newDocument.DocumentType)
                .Count(doc => doc["DocumentCreatorUserName"] == newDocument.DocumentCreator)
                .Should().Be(1);
        }

        [Test]
        public async Task GetAllRecordsReturnsAllDocumentRecordsIfThereAreLessThanTheLimit()
        {
            var savedDocumentOne = await AddDocumentToDatabase(RandomDocumentDetails());
            var savedDocumentTwo = await AddDocumentToDatabase(RandomDocumentDetails());

            var expectedResponse = new List<DocumentDetails> {savedDocumentOne, savedDocumentTwo};

            var response = await _dbGateway.GetAllRecords(10, null);

            response.Should().BeEquivalentTo(expectedResponse, options => options.Excluding(d => d.Log));
        }

        [Test]
        public async Task GetAllRecordsReturnsAllDocumentRecordsWithinLimit()
        {
            var savedDocumentOne = await AddDocumentToDatabase(RandomDocumentDetails());
            var savedDocumentTwo = await AddDocumentToDatabase(RandomDocumentDetails());
            var savedDocumentThree = await AddDocumentToDatabase(RandomDocumentDetails());

            var expectedResponse = new List<DocumentDetails> {savedDocumentTwo, savedDocumentThree};

            var response = await _dbGateway.GetAllRecords(2, null);

            response.Should().BeEquivalentTo(expectedResponse, options => options.Excluding(d => d.Log));
        }

        [Test]
        public async Task GetAllRecordsReturnsAllDocumentRecordsLaterThanTheCursor()
        {
            var savedDocumentOne = await AddDocumentToDatabase(RandomDocumentDetails());
            var savedDocumentTwo = await AddDocumentToDatabase(RandomDocumentDetails());
            var savedDocumentThree = await AddDocumentToDatabase(RandomDocumentDetails());

            var expectedResponse = new List<DocumentDetails> {savedDocumentOne};

            var response = await _dbGateway.GetAllRecords(2, savedDocumentTwo.Id);

            response.Should().BeEquivalentTo(expectedResponse, options => options.Excluding(d => d.Log));
        }

        [Test]
        public async Task AddRecordForDocumentId_SavesARecordToTheDatabase_WithCorrectTimestamp()
        {
            var newDocument = RandomDocumentDetails();
            await _dbGateway.SaveDocument(newDocument);
            var expectedTimestamp = DateTime.Parse(GetCurrentTimestamp());

            var response = await GetItemsFromDatabase();
            var savedTimestamp = GetTimestampForDocumentId(response, newDocument);
            savedTimestamp.Should().BeCloseTo(expectedTimestamp, 1000);
        }

        [Test]
        public async Task AddRecordForDocumentId_ReturnsTheTimestampCreated()
        {
            var newDocument = RandomDocumentDetails();
            var response = await _dbGateway.SaveDocument(newDocument);
            var currentTimestamp = DateTime.Parse(GetCurrentTimestamp());

            DateTime.Parse(response).Should().BeCloseTo(currentTimestamp, 1000);
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

            savedItems.Count(doc => doc["CominoDocumentNumber"] == document1.CominoDocumentNumber).Should().Be(1);
            savedItems.Count(doc => doc["CominoDocumentNumber"] == document2.CominoDocumentNumber).Should().Be(1);
        }

        [Test]
        public async Task AddRecordForDocumentId_IfTwoRecordsAddedAtTheSameTime_ReturnsTimestampFromRetry()
        {
            var currentTimestamp = GetCurrentTimestamp();

            var document1 = RandomDocumentDetails();
            await AddDocumentToDatabase(document1, currentTimestamp);

            var document2 = RandomDocumentDetails();
            var response = await _dbGateway.SaveDocument(document2);

            DateTime.Parse(response).Should().BeAfter(DateTime.Parse(currentTimestamp));
        }

        [Test]
        public async Task GetRecordByTimeStamp_RetrievesRecordFromDb()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());

            var response = await _dbGateway.GetRecordByTimeStamp(savedDocument.Id);

            response.Should().BeEquivalentTo(savedDocument);
        }

        [Test]
        public async Task RetrieveDocumentAndSetStatusToProcessing_UpdatesADocumentsStatus()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());

            await _dbGateway.RetrieveDocumentAndSetStatusToProcessing(savedDocument.Id);

            var savedDoc = await DatabaseClient.DocumentTable.GetItemAsync(savedDocument.Id);
            savedDoc["Status"].ToString().Should().Be(LetterStatusEnum.Processing.ToString());
        }

        [Test]
        public async Task RetrieveDocumentAndSetStatusToProcessing_ReturnsTheDocument()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());

            var response = await _dbGateway.RetrieveDocumentAndSetStatusToProcessing(savedDocument.Id);

            response.Should().BeEquivalentTo(savedDocument);
        }

        [Test]
        public async Task RetrieveDocumentAndSetStatusToProcessing_IfDocumentAlreadyProcessing_ReturnsNull()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails(),
                status: LetterStatusEnum.Processing);

            var response = await _dbGateway.RetrieveDocumentAndSetStatusToProcessing(savedDocument.Id);

            response.Should().BeNull();
        }

        [Test]
        public async Task RetrieveDocumentAndSetStatusToProcessing_IfDocumentDoesntExist_ReturnsNull()
        {
            var response = await _dbGateway.RetrieveDocumentAndSetStatusToProcessing("317286387");

            response.Should().BeNull();
        }

        [Test]
        public async Task GetLettersWaitingForGovNotify_GetAnyLettersMarkedAsSentToGovNotify()
        {
            var document1ToReturn = await AddDocumentToDatabase(RandomDocumentDetails(), status: LetterStatusEnum.SentToGovNotify);
            var document2ToReturn = await AddDocumentToDatabase(RandomDocumentDetails(), status: LetterStatusEnum.SentToGovNotify);

            var documentNotToReturn =
                await AddDocumentToDatabase(RandomDocumentDetails(), status: LetterStatusEnum.Processing);

            var response = await _dbGateway.GetLettersWaitingForGovNotify();
            response.Should().BeEquivalentTo(new List<DocumentDetails> {document1ToReturn, document2ToReturn}, options => options.Excluding(x => x.Log));
        }

        [Test]
        public async Task GetLettersWaitingForGovNotify_GetAnyLettersMarkedAsGovNotifyPendingVirusCheck()
        {
            var document1ToReturn = await AddDocumentToDatabase(RandomDocumentDetails(), status: LetterStatusEnum.GovNotifyPendingVirusCheck);
            var document2ToReturn = await AddDocumentToDatabase(RandomDocumentDetails(), status: LetterStatusEnum.GovNotifyPendingVirusCheck);

            var documentNotToReturn =
                await AddDocumentToDatabase(RandomDocumentDetails(), status: LetterStatusEnum.Processing);

            var response = await _dbGateway.GetLettersWaitingForGovNotify();
            response.Should().BeEquivalentTo(new List<DocumentDetails> {document1ToReturn, document2ToReturn}, options => options.Excluding(x => x.Log));
        }

        [Test]
        public async Task UpdateStatus_SetsNewStatusForADocument()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var newStatus = LetterStatusEnum.ProcessingError;

            await _dbGateway.UpdateStatus(savedDocument.Id, newStatus);

            var savedDoc = await DatabaseClient.DocumentTable.GetItemAsync(savedDocument.Id);
            savedDoc["Status"].ToString().Should().Be(newStatus.ToString());
        }

        [Test]
        public async Task UpdateStatus_ReturnsTrueIfStatusIsChanged()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var newStatus = LetterStatusEnum.ProcessingError;

            var response = await _dbGateway.UpdateStatus(savedDocument.Id, newStatus);
            response.StatusUpdated.Should().BeTrue();
        }

        [Test]
        public async Task UpdateStatus_ReturnsFalseIfStatusIsNotChanged()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails(), status: LetterStatusEnum.ProcessingError);
            var newStatus = LetterStatusEnum.ProcessingError;

            var response = await _dbGateway.UpdateStatus(savedDocument.Id, newStatus);
            response.StatusUpdated.Should().BeFalse();
        }

        [Test]
        public async Task GetDocumentsThatAreReadyForGovNotify_GetsCorrectDocuments()
        {

            var readyDocument = RandomDocumentDetails();
            var savedReadyDocument = await AddDocumentToDatabase(readyDocument, null, LetterStatusEnum.ReadyForGovNotify);

            var notReadyDocument = RandomDocumentDetails();
            var savedNotReadyDocument = await AddDocumentToDatabase(notReadyDocument, null, LetterStatusEnum.Processing);

            var expected = new List<DocumentDetails> { savedReadyDocument };
            var received = await _dbGateway.GetDocumentsThatAreReadyForGovNotify();

            expected.Should().BeEquivalentTo(received, options => options.Excluding(d => d.Log));
        }

        [Test]
        public async Task LogMessage_WhenDoesntExist_CreatesLog()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());
            var logMessage = "Something has happened";
            await _dbGateway.LogMessage(savedDocument.Id, logMessage);
            var savedItems = await GetItemsFromDatabase();
            var log = GetLog(savedItems, savedDocument.Id);

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
                    {"Yesterday", new AttributeValue {S = "I was made"}},
                }
            };

            await UpdateLog(savedDocument, previousLog);

            var logMessage = "Something has happened";
            await _dbGateway.LogMessage(savedDocument.Id, logMessage);

            var savedItems = await GetItemsFromDatabase();
            var log = GetLog(savedItems, savedDocument.Id);

            log.Values.Select(s => s.ToString()).Should().Contain(logMessage);
        }

        [Test]
        public async Task LogMessage_WhenItemDoesntExist_DoesNothing()
        {
            var savedDocument = await AddDocumentToDatabase(RandomDocumentDetails());

            var logMessage = "Something has happened";
            await _dbGateway.LogMessage("35672157", logMessage);

            var savedItems = await GetItemsFromDatabase();

            savedItems.First(i => i["InitialTimestamp"] == savedDocument.Id).Should().NotContainKey("Log");
            savedItems.Count().Should().Be(1);
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
                    {"yesterday", new AttributeValue {S = "I was made"}},
                    {"last week", new AttributeValue {S = "then this happened"}},
                    {"earlier today", new AttributeValue {S = "then something else happened"}},
                    {currentTime, new AttributeValue {S = "that was the end"}}
                }
            };

            await UpdateLog(savedDocument, logEntries);

            var expectedLog = new Dictionary<string, string>
            {
                {"yesterday", "I was made"},
                {"last week", "then this happened"},
                {"earlier today", "then something else happened"},
                {currentTime, "that was the end"}
            };
            var receivedLog = _dbGateway.GetLogForDocument(savedDocument.Id);
            receivedLog.Entries.Should().BeEquivalentTo(expectedLog);
        }

        private async Task UpdateLog(DocumentDetails savedDocument, AttributeValue logEntries)
        {
            var update = new UpdateItemRequest
            {
                TableName = DatabaseClient.DocumentTable.TableName,
                UpdateExpression = "SET #atr = :val",
                Key = new Dictionary<string, AttributeValue>
                    {{"InitialTimestamp", new AttributeValue {S = savedDocument.Id}}},
                ExpressionAttributeNames = new Dictionary<string, string> {{"#atr", "Log"}},
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {{":val", logEntries}}
            };
            await DatabaseClient.DynamoDBClient.UpdateItemAsync(update);
        }

        private static Document GetLog(List<Document> savedItems, string savedAt)
        {
            return savedItems.First(i => i["InitialTimestamp"] == savedAt)["Log"].AsDocument();
        }

        private static string GetCurrentTimestamp()
        {
            return DateTime.UtcNow.ToString("O");
        }

        private DocumentDetails RandomDocumentDetails()
        {
            return new DocumentDetails
            {
                CominoDocumentNumber = _fixture.Create<string>(),
                DocumentCreator = _fixture.Create<string>(),
                LetterType = _fixture.Create<string>(),
                DocumentType = _fixture.Create<string>(),
            };
        }

        private async Task<DocumentDetails> AddDocumentToDatabase(DocumentDetails document,
            string currentTimestamp = null, LetterStatusEnum status = LetterStatusEnum.Waiting)
        {
            var timestamp = currentTimestamp ?? GetCurrentTimestamp();
            var documentItem = new Document
            {
                ["CominoDocumentNumber"] = document.CominoDocumentNumber,
                ["DocumentCreatorUserName"] = document.DocumentCreator,
                ["InitialTimestamp"] = timestamp,
                ["LetterType"] = document.LetterType,
                ["DocumentType"] = document.DocumentType,
                ["Status"] = status.ToString()
            };

            await DatabaseClient.DocumentTable.PutItemAsync(documentItem);

            document.Id = timestamp;
            document.Status = status;
            document.Log = new Dictionary<string, string>();
            return document;
        }

        private static DateTime GetTimestampForDocumentId(List<Document> savedItems, DocumentDetails document)
        {
            return DateTime.Parse(savedItems.First(doc => doc["CominoDocumentNumber"] == document.CominoDocumentNumber)["InitialTimestamp"]);
        }

        private async Task<List<Document>> GetItemsFromDatabase()
        {
            var scanFilter = new ScanFilter();

            var search = DatabaseClient.DocumentTable.Scan(scanFilter);
            return await search.GetRemainingAsync();
        }
    }
}
