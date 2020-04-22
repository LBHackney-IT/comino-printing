using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace Gateways
{
    public interface IDynamoDBHandler
    {
        Table DocumentTable { get; }
        AmazonDynamoDBClient DatabaseClient { get; }
    }
}