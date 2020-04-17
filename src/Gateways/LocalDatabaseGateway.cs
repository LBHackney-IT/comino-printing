using System;
using Amazon.DynamoDBv2;

namespace Gateways
{
    public class LocalDatabaseGateway
    {
        public LocalDatabaseGateway(IDynamoDatabaseClient testDatabase)
        {
            throw new NotImplementedException();
        }
    }

    public interface IDynamoDatabaseClient
    {
        AmazonDynamoDBClient Client { get; }
    }

    public class DynamoDatabaseHandler : IDynamoDatabaseClient
    {
        private readonly AmazonDynamoDBClient _client;

        public DynamoDatabaseHandler(AmazonDynamoDBConfig config)
        {
            _client = new AmazonDynamoDBClient(config);
        }

        public AmazonDynamoDBClient Client => _client;
    }
}