using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace Gateways
{
    public class DynamoDBHandler : IDynamoDBHandler
    {
        public DynamoDBHandler(AmazonDynamoDBConfig dynamoConfig)
        {
            var client = new AmazonDynamoDBClient(dynamoConfig);
            DocumentTable = Table.LoadTable(client, "Documents");
        }

        public Table DocumentTable { get; }
    }
}