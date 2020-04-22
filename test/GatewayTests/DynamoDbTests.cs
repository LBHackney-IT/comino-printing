using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Gateways;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GatewayTests
{
    public class DynamoDbTests
    {
        protected DynamoDBHandler DatabaseClient;

        [SetUp]
        public void DynamoDbSetUp()
        {
            Environment.SetEnvironmentVariable("DYNAMODB_SET_DUMMY_AUTH", "true");

            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = "eu-west-2",
                ServiceURL = "http://localhost:8000",
            };

            var tableName = Environment.GetEnvironmentVariable("LETTERS_TABLE_NAME") ?? "comino-printing-test-letters-2";
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