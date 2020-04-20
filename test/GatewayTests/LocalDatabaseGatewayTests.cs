using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using AutoFixture;
using FluentAssertions;
using Gateways;
using NUnit.Framework;
using Usecases.Domain;

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
        [Ignore("To Fix")]
        public async Task AddRecordForDocumentId_SavesARecordToTheDatabase_WithDocumentIdAndCreator()
        {
            var newDocument = RandomDocumentDetails();
            await _dbGateway.SaveDocument(newDocument);

            var response = await GetItemsFromDatabase();

            response
                .Where(doc => doc["DocumentId"] == newDocument.DocumentId)
                .Count(doc => doc["DocumentCreator"] == newDocument.DocumentCreator)
                .Should().Be(1);
        }

        [Test]
        [Ignore("To Fix")]
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
        [Ignore("To Fix")]
        public async Task AddRecordForDocumentId_ReturnsTheTimestampCreated()
        {
            var newDocument = RandomDocumentDetails();
            var response = await _dbGateway.SaveDocument(newDocument);
            var currentTimestamp = GetCurrentTimestamp();

            Convert.ToInt32(response).Should().BeCloseTo(currentTimestamp, 1);
        }

        [Test]
        [Ignore("To Fix")]
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
        [Ignore("To Fix")]
        public async Task AddRecordForDocumentId_IfTwoRecordsAddedAtTheSameTime_ReturnsTimestampFromRetry()
        {
            var currentTimestamp = GetCurrentTimestamp();

            var document1 = RandomDocumentDetails();
            await AddDocumentToDatabase(document1, currentTimestamp);

            var document2 = RandomDocumentDetails();
            var response = await _dbGateway.SaveDocument(document2);

            Convert.ToInt32(response).Should().BeGreaterThan(currentTimestamp);
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
                DocumentCreator = _fixture.Create<string>()
            };
        }

        private async Task AddDocumentToDatabase(DocumentDetails document1, int currentTimestamp)
        {
            var documentItem = new Document
            {
                ["DocumentId"] = document1.DocumentId,
                ["DocumentCreator"] = document1.DocumentCreator,
                ["InitialTimestamp"] = currentTimestamp.ToString(),
                ["Status"] = "Waiting"
            };
            await DatabaseClient.DocumentTable.PutItemAsync(documentItem);
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
