using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Gateways;
using NUnit.Framework;

namespace GatewayTests
{
    public class DynamoDbTests
    {
        protected DynamoDBHandler DatabaseClient;

        [SetUp]
        public void DynamoDbSetUp()
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000",
            };

            var tableName = Environment.GetEnvironmentVariable("LETTERS_TABLE_NAME") ?? "comino-printing-test-letters";
            DatabaseClient = new DynamoDBHandler(config, tableName);
        }

        [TearDown]
        public async Task DynamoDbTearDown()
        {
            var scanFilter = new ScanFilter();

            var search = DatabaseClient.DocumentTable.Scan(scanFilter);
            var items = await search.GetRemainingAsync();
            foreach (var document in items)
            {
                await DatabaseClient.DocumentTable.DeleteItemAsync(document);
            }
        }
    }
}