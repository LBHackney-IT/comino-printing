using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;

namespace Gateways
{
    public class DynamoDBHandler : IDynamoDBHandler
    {
        public DynamoDBHandler(AmazonDynamoDBConfig dynamoConfig, string tableName)
        {
            // TEMPORARY: debugging AWS SDK DynamoDB tests - hanging due to auth issue?
            if (Environment.GetEnvironmentVariable("DYNAMODB_SET_DUMMY_AUTH") == "true")
            {
                var credentials = new BasicAWSCredentials("TestAccessKey", "TestSecretKey");
                var client = new AmazonDynamoDBClient(credentials, dynamoConfig);
                Console.WriteLine($"> setting DocumentTable: {tableName}");
                DocumentTable = Table.LoadTable(client, tableName);
                DatabaseClient = client;
            }
            else
            {
                var client = new AmazonDynamoDBClient(dynamoConfig);
                DocumentTable = Table.LoadTable(client, tableName);
            }
        }

        public AmazonDynamoDBClient DatabaseClient { get; }
        public Table DocumentTable { get; }
    }
}
