using Amazon;
using Amazon.DynamoDBv2;
using Gateways;
using NUnit.Framework;

namespace GatewayTests
{
    public class LocalDatabaseGatewayTests
    {
        [SetUp]
        public void SetUp()
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000"
            };
            var testClient = new DynamoDatabaseHandler(config);

            var _dbGateway = new LocalDatabaseGateway(testClient);
        }
    }
}