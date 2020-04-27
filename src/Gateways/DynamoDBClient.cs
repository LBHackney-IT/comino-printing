using Amazon.DynamoDBv2;

namespace Gateways
{
    public class DynamoDBClient
    {
        public AmazonDynamoDBClient Client { get; set; }

        public DynamoDBClient(AmazonDynamoDBConfig dynamoConfig)
        {
            Client = new AmazonDynamoDBClient(dynamoConfig);
        }
    }
}
