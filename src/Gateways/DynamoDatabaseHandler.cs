using Amazon.DynamoDBv2;

namespace Gateways
{
    public class DynamoDatabaseHandler : IDynamoDatabaseClient
    {
        public DynamoDatabaseHandler(AmazonDynamoDBConfig config)
        {
            Client = new AmazonDynamoDBClient(config);
        }

        public AmazonDynamoDBClient Client { get; }
    }
}