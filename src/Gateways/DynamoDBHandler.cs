using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace Gateways
{
    public class DynamoDBHandler : IDynamoDBHandler
    {
        public DynamoDBHandler(AmazonDynamoDBConfig dynamoConfig, string tableName)
        {
            var client = new AmazonDynamoDBClient(dynamoConfig);
            DocumentTable = Table.LoadTable(client, tableName);
        }

        public Table DocumentTable { get; }
    }
}
