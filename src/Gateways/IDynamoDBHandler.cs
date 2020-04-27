using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace Gateways
{
    public interface IDynamoDBHandler
    {
        Table DocumentTable { get; }
        AmazonDynamoDBClient DynamoDBClient { get; }
    }
}