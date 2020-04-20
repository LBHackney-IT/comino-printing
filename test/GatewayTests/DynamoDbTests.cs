using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Gateways;
using NUnit.Framework;

namespace GatewayTests
{
    public class DynamoDbTests
    {
        protected DynamoDBHandler DatabaseClient;

        [SetUp]
        public async Task DynamoDbSetUp()
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000",
            };

            var tableName = Environment.GetEnvironmentVariable("LETTERS_TABLE_NAME");
            if (tableName == null)
            {
                var client = new AmazonDynamoDBClient(config);
                var tables = await client.ListTablesAsync();
                client.Dispose();
                tableName = tables.TableNames.First();
            }
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