using Amazon.DynamoDBv2;

namespace Gateways
{
    public interface IDynamoDatabaseClient
    {
        AmazonDynamoDBClient Client { get; }
    }
}