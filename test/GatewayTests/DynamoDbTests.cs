using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Gateways;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GatewayTests
{
    public class DynamoDbTests
    {
        protected DynamoDBHandler DatabaseClient;
        protected string tableName = Environment.GetEnvironmentVariable("LETTERS_TABLE_NAME") ?? "comino-printing-test-letters-2";

        [SetUp]
        public void DynamoDbSetUp()
        {
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.EUWest2,
                ServiceURL = "http://localhost:8000", //tell SDK to use local database
            };

            DynamoDBClient dynamoDBClient = new DynamoDBClient(config);

            var request = new ListTablesRequest();
            var listTablesResponse = dynamoDBClient.Client.ListTablesAsync(request).Result;

            if (!listTablesResponse.TableNames.Contains(tableName))
            {
                var createTableRequest = new CreateTableRequest
                {
                    AttributeDefinitions = new List<AttributeDefinition>()
                        {
                            new AttributeDefinition
                            {
                                AttributeName = "InitialTimestamp",
                                AttributeType = "S"
                            }
                        },
                    KeySchema = new List<KeySchemaElement>
                        {
                            new KeySchemaElement
                            {
                                AttributeName = "InitialTimestamp",
                                KeyType = "HASH"
                            }
                        },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 20,
                        WriteCapacityUnits = 10
                    },
                    TableName = tableName
                };
                var response = dynamoDBClient.Client.CreateTableAsync(createTableRequest).Result;
            }

            DatabaseClient = new DynamoDBHandler(tableName, dynamoDBClient);
        }

        [TearDown]
        public async Task DynamoDbTearDown()
        {
            //delete the table created above
            var request = new ListTablesRequest();
            var listTablesResponse = DatabaseClient.DynamoDBClient.ListTablesAsync(request).Result;

            if (listTablesResponse.TableNames.Contains(tableName))
            {
                await DatabaseClient.DynamoDBClient.DeleteTableAsync(tableName);
            }
        }
    }
}