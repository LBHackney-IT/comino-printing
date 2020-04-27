using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System;

namespace Gateways
{
    public class DynamoDBHandler : IDynamoDBHandler
    {
        private readonly AmazonDynamoDBClient _client;

        public DynamoDBHandler(string tableName, DynamoDBClient dynamoDBClient)
        {
            _client = dynamoDBClient.Client;

            Console.WriteLine($"> setting DocumentTable: {tableName}");
            DocumentTable = Table.LoadTable(_client, tableName);
            DynamoDBClient = _client;
        }

        public AmazonDynamoDBClient DynamoDBClient { get; }
        public Table DocumentTable { get; }
    }
}
