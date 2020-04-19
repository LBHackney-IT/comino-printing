using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
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
        public async Task AddRecordForDocumentId_SavesARecordToTheDatabase_WithDocumentIdAndCreator()
        {
            var newDocument = new DocumentDetails
            {
                DocumentId = _fixture.Create<string>(),
                DocumentCreator = _fixture.Create<string>()
            };
            await _dbGateway.SaveDocument(newDocument);

            var scanFilter = new ScanFilter();

            var search = DatabaseClient.DocumentTable.Scan(scanFilter);
            var response = await search.GetRemainingAsync();

            response
                .Where(doc => doc["DocumentId"] == newDocument.DocumentId)
                .Count(doc => doc["DocumentCreator"] == newDocument.DocumentCreator)
                .Should().Be(1);
        }

        [Test]
        public async Task AddRecordForDocumentId_SavesARecordToTheDatabase_WithCorrectTimestamp()
        {
            var newDocument = new DocumentDetails
            {
                DocumentId = _fixture.Create<string>(),
                DocumentCreator = _fixture.Create<string>()
            };
            await _dbGateway.SaveDocument(newDocument);
            var currentTimestamp = Convert.ToInt32((DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds);
            var scanFilter = new ScanFilter();
            scanFilter.AddCondition("DocumentId", ScanOperator.Equal, newDocument.DocumentId);

            var search = DatabaseClient.DocumentTable.Scan(scanFilter);
            var response = await search.GetRemainingAsync();

            var timestamp = (int) response.First(doc => doc["DocumentId"] == newDocument.DocumentId)["InitialTimestamp"];
            timestamp.Should().BeCloseTo(currentTimestamp, 1);
        }

        [Ignore("In Progress")]
        [Test]
        public void AddRecordForDocumentId_ReturnsTheTimestampCreated()
        {
            var newDocument = new DocumentDetails
            {
                DocumentId = _fixture.Create<string>(),
                DocumentCreator = _fixture.Create<string>()
            };
            var response = _dbGateway.SaveDocument(newDocument);
        }

        [Ignore("In Progress")]
        [Test]
        public void AddRecordForDocumentId_IfDatabaseReturnsAnError_TriesAgain()
        {
            
        }
    }
}