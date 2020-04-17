using System;

namespace Gateways
{
    public class LocalDatabaseGateway
    {
        private DynamoDbClient dynamoDb() {
//            var endpoint = Environment.GetEnvironmentVariable("ENDPOINT_OVERRIDE");

            DynamoDbClientBuilder builder = DynamoDbClient.builder();
            builder.httpClient(ApacheHttpClient.builder().build());
            if (endpoint != null && !endpoint.isEmpty()) {
                builder.endpointOverride(URI.create(endpoint));
            }

            return builder.build();
        }
    }
}